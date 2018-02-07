using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using log4net;

namespace Kartverket.MetadataEditor.Models.OpenData
{
    internal class OpenMetadataService : IOpenMetadataService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IOpenMetadataFetcher _metadataFetcher;

        private IMetadataService _metadataService;

        public OpenMetadataService(IMetadataService metadataService, IOpenMetadataFetcher metadataFetcher)
        {
            _metadataService = metadataService;
            _metadataFetcher = metadataFetcher;
        }

        public async Task<int> SynchronizeMetadata()
        {
            Log.Info("Synching open metadata initiated");

            var endPoints = new List<OpenMetadataEndpoint>();
            endPoints.Add(new OpenMetadataEndpoint {Url = "https://oslokommune-bym.opendata.arcgis.com/data.json", OrganizationName = "Oslo kommune"});
            endPoints.Add(new OpenMetadataEndpoint {Url = "http://data-tromso.opendata.arcgis.com/data.json", OrganizationName = "Tromsø"});

            Log.Info("List of endpoints: ");
            var numberOfUpdatedMetadata = 0;
            foreach (var endPoint in endPoints)
            {
                var updateCount = await SynchronizeMetadata(endPoint).ConfigureAwait(false);
                numberOfUpdatedMetadata += updateCount;
            }

            Log.Info("Number of metadata updated: " + numberOfUpdatedMetadata);
            return numberOfUpdatedMetadata;
        }

        public async Task<int> SynchronizeMetadata(OpenMetadataEndpoint endpoint)
        {
            var openMetadata = await _metadataFetcher.FetchMetadataAsync(endpoint).ConfigureAwait(false);

            var numberOfMetadataCreatedUpdated = 0;
            foreach (var dataset in openMetadata.dataset)
                if (CreateOrUpdateMetadata(dataset, endpoint))
                    numberOfMetadataCreatedUpdated++;

            return numberOfMetadataCreatedUpdated;
        }

        private bool CreateOrUpdateMetadata(Dataset dataset, OpenMetadataEndpoint endpoint)
        {
            string identifier = null;
            try
            {
                identifier = GetIdentifierFromUri(dataset.identifier);
                var metadataModel = _metadataService.GetMetadataModel(identifier);

                if (metadataModel == null)
                {
                    InsertOpenMetadata(identifier, dataset, endpoint);
                    Log.Info(
                        $"Created metadata entry for open dataset: [identifier={identifier}, title={dataset.title}, organization={endpoint.OrganizationName}");
                    metadataModel = _metadataService.GetMetadataModel(identifier);
                }

                MapDatasetToMetadataViewModel(dataset, metadataModel, endpoint);
                _metadataService.SaveMetadataModel(metadataModel, SecurityClaim.GetUsername());
                Log.Info(
                    $"Updated metadata entry for open dataset: [identifier={identifier}, title={metadataModel.Title}, organization={endpoint.OrganizationName}");
            }
            catch (Exception e)
            {
                Log.Error(
                    $"Error while creating or updating open metadata with identifier={identifier} for endpoint={endpoint}. {e.Message}",
                    e);
                return false;
            }

            return true;
        }

        private void InsertOpenMetadata(string identifier, Dataset dataset, OpenMetadataEndpoint openMetadataEndpoint)
        {
            var newMetadata = new MetadataCreateViewModel();
            newMetadata.Uuid = identifier;
            newMetadata.Type = "dataset";
            newMetadata.Title = dataset.title;
            newMetadata.MetadataContactOrganization =
                !string.IsNullOrEmpty(openMetadataEndpoint.OrganizationName)
                    ? openMetadataEndpoint.OrganizationName
                    : dataset.publisher.name;
            newMetadata.MetadataContactName = dataset.contactPoint.fn;
            newMetadata.MetadataContactEmail = dataset.contactPoint.hasEmail.Replace("mailto:", "");
            var uuid = _metadataService.CreateMetadata(newMetadata, SecurityClaim.GetUsername());
            Log.Info("Created open metadata uuid: " + uuid);
        }


        private void MapDatasetToMetadataViewModel(Dataset dataset, MetadataViewModel model,
            OpenMetadataEndpoint openMetadataEndpoint)
        {
            model.Title = dataset.title;
            DateTime modified;
            if (DateTime.TryParse(dataset.modified.ToString(), out modified)) model.DateUpdated = modified;

            DateTime issued;
            if (DateTime.TryParse(dataset.issued, out issued)) model.DatePublished = issued;
            
            model.ContactMetadata.Organization =
                !string.IsNullOrEmpty(openMetadataEndpoint.OrganizationName)
                    ? openMetadataEndpoint.OrganizationName
                    : dataset.publisher.name;
            model.ContactMetadata.Email = dataset.contactPoint.hasEmail.Replace("mailto:", "");
            model.ContactMetadata.Name = dataset.contactPoint.fn;

            model.ContactPublisher.Organization = model.ContactMetadata.Organization;
            model.ContactPublisher.Email = model.ContactMetadata.Email;
            model.ContactPublisher.Name = model.ContactMetadata.Name;

            model.ContactOwner.Organization = model.ContactMetadata.Organization;
            model.ContactOwner.Email = model.ContactMetadata.Email;
            model.ContactOwner.Name = model.ContactMetadata.Name;

            model.Abstract = dataset.description;

            if (dataset.keyword != null && dataset.keyword.Length > 0)
                model.KeywordsOther = dataset.keyword.ToList();

            model.DistributionsFormats =
                GetDistributionsFormats(dataset.distribution, model.ContactMetadata.Organization);

            if (string.IsNullOrEmpty(model.MaintenanceFrequency))
                model.MaintenanceFrequency = "unknown";

            var spatial = dataset.spatial.Split(',');
            if (spatial.Length == 4)
            {
                model.BoundingBoxWest = spatial[0];
                model.BoundingBoxSouth = spatial[1];
                model.BoundingBoxEast = spatial[2];
                model.BoundingBoxNorth = spatial[3];
            }

            model.AccessConstraints = "no restrictions";
        }

        private List<SimpleDistribution> GetDistributionsFormats(Distribution[] distributions, string organization)
        {
            var formatDistributions = new List<SimpleDistribution>();

            foreach (var distribution in distributions)
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
                    });
            }

            return formatDistributions;
        }

        private string MapToGeonorgeProtocol(Distribution distribution)
        {
            if (!string.IsNullOrEmpty(distribution.accessURL) && distribution.format.ToLower().Contains("wms"))
                return "OGC:WMS";
            if (!string.IsNullOrEmpty(distribution.accessURL) && distribution.title.ToLower().Contains("rest api"))
                return "W3C:REST";
            return "WWW:DOWNLOAD-1.0-http--download";
        }

        internal static string GetIdentifierFromUri(string identifier)
        {
            return identifier.Split('/').Last();
        }
    }

    public class OpenMetadataEndpoint
    {
        public string Url { get; set; }
        public string OrganizationName { get; set; }

        public override string ToString()
        {
            return $"{OrganizationName} [url={Url}]";
        }
    }
}