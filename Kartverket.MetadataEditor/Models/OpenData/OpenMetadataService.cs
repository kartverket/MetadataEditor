using GeoNorgeAPI;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Kartverket.MetadataEditor.Models.OpenData
{
    public class OpenMetadataService : IOpenMetadataService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));
        private static readonly HttpClient _httpClient = new HttpClient();
        static List<EndPoint> _endPoints;
        string _endPointOslo = "https://oslokommune-bym.opendata.arcgis.com/data.json";
        string _endPointTromso = "http://data-tromso.opendata.arcgis.com/data.json";
        string username;

        private IMetadataService _metadataService;

        public void SyncData(IMetadataService metadataService)
        {
            _endPoints = new List<EndPoint>();
            _endPoints.Add(new EndPoint {  Url = _endPointOslo, OrganizationName = "Oslo kommune" });
            _endPoints.Add(new EndPoint { Url = _endPointTromso });

            username = GetUsername();

            _metadataService = metadataService;

            foreach(var endPoint in _endPoints)
            {
                SyncEndpoint(endPoint);
            }
        }

        private void SyncEndpoint(EndPoint endPoint)
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var result = _httpClient.GetAsync(endPoint.Url).Result;
            if (result.IsSuccessStatusCode)
            {

                var metadata = result.Content.ReadAsAsync<Metadata>().Result;
                MetadataViewModel model = new MetadataViewModel();
                model.HierarchyLevel = "dataset";
                foreach (var dataset in metadata.dataset)
                {
                    string identifier = null;

                    try
                    {
                        identifier = GetIdentifierFromUri(dataset.identifier);
                        model = _metadataService.GetMetadataModel(identifier);
                        model.Title = dataset.title;
                        DateTime modified;
                        if (DateTime.TryParse(dataset.modified.ToString(), out modified))
                        {
                            model.DateUpdated = modified;
                        }
                        DateTime issued;
                        if (DateTime.TryParse(dataset.issued.ToString(), out issued))
                        {
                            model.DatePublished = issued;
                        }
                        model.ContactMetadata.Organization = 
                            !string.IsNullOrEmpty(endPoint.OrganizationName) ? endPoint.OrganizationName : dataset.publisher.name;
                        model.ContactMetadata.Email = dataset.contactPoint.hasEmail.Replace("mailto:", "");
                        model.ContactMetadata.Name = dataset.contactPoint.fn;

                        model.ContactPublisher.Organization = model.ContactMetadata.Organization;
                        model.ContactPublisher.Email = model.ContactMetadata.Email;
                        model.ContactPublisher.Name = model.ContactMetadata.Name;

                        model.ContactOwner.Organization = model.ContactMetadata.Organization;
                        model.ContactOwner.Email = model.ContactMetadata.Email;
                        model.ContactOwner.Name = model.ContactMetadata.Name;

                        model.Abstract = dataset.description;

                        if(dataset.keyword != null && dataset.keyword.Length > 0)
                            model.KeywordsOther = dataset.keyword.ToList();

                        model.DistributionsFormats = GetDistributionsFormats(dataset.distribution, model.ContactMetadata.Organization);

                        if (string.IsNullOrEmpty(model.MaintenanceFrequency))
                            model.MaintenanceFrequency = "unknown";

                        var spatial = dataset.spatial.Split(',');
                        if(spatial.Length == 4)
                        {
                            model.BoundingBoxWest = spatial[0];
                            model.BoundingBoxSouth = spatial[1];
                            model.BoundingBoxEast = spatial[2];
                            model.BoundingBoxNorth = spatial[3];
                        }

                        model.AccessConstraints = "no restrictions";

                        _metadataService.SaveMetadataModel(model, username);
                        Log.Info("Updated open metadata uuid: " + model.Uuid);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Metadata cannot be null.\r\nParameternavn: md" && !string.IsNullOrEmpty(identifier))
                        {
                            MetadataCreateViewModel newMetadata = new MetadataCreateViewModel();
                            newMetadata.Uuid = identifier;
                            newMetadata.Type = "dataset";
                            newMetadata.Title = dataset.title;
                            newMetadata.MetadataContactOrganization = 
                                !string.IsNullOrEmpty(endPoint.OrganizationName) ? endPoint.OrganizationName : dataset.publisher.name;
                            newMetadata.MetadataContactName = dataset.contactPoint.fn;
                            newMetadata.MetadataContactEmail = dataset.contactPoint.hasEmail.Replace("mailto:", "");
                            string uuid = _metadataService.CreateMetadata(newMetadata, username);
                            Log.Info("Created open metadata uuid: " + uuid);
                        }
                    }
                }
            }
        }

        private List<SimpleDistribution> GetDistributionsFormats(Distribution[] distributions, string organization)
        {
            List<SimpleDistribution> formatDistributions = new List<SimpleDistribution>();

            foreach(var distribution in distributions)
            {
                var url = distribution.downloadURL;
                if (string.IsNullOrEmpty(url))
                    url = distribution.accessURL;

                formatDistributions.Add( 
                    new SimpleDistribution
                    {
                        Organization = organization,
                        URL = url,
                        FormatName = distribution.format,
                        Protocol = MapToGeonorgeProtocol(distribution)
                    } );
            }

            return formatDistributions;
        }

        private string MapToGeonorgeProtocol(Distribution distribution)
        {
            if (!string.IsNullOrEmpty(distribution.accessURL) && distribution.format.ToLower().Contains("wms"))
                return "OGC:WMS";
            else if (!string.IsNullOrEmpty(distribution.accessURL) && distribution.title.ToLower().Contains("rest api"))
                return "W3C:REST";
            else
                return "WWW:DOWNLOAD-1.0-http--download";
        }

        private string GetIdentifierFromUri(string identifier)
        {
            return identifier.Split('/').Last();
        }

        private string GetUsername()
        {
            return GetSecurityClaim("username");
        }

        private string GetSecurityClaim(string type)
        {
            string result = null;
            foreach (var claim in System.Security.Claims.ClaimsPrincipal.Current.Claims)
            {
                if (claim.Type == type && !string.IsNullOrWhiteSpace(claim.Value))
                {
                    result = claim.Value;
                    break;
                }
            }

            // bad hack, must fix BAAT
            if (!string.IsNullOrWhiteSpace(result) && type.Equals("organization") && result.Equals("Statens kartverk"))
            {
                result = "Kartverket";
            }

            return result;
        }

        public void SyncData()
        {
            throw new NotImplementedException();
        }
    }

    class EndPoint
    {
        public string Url { get; set; }
        public string OrganizationName { get; set; }
    }
}