using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using log4net;
using System.Data.Entity;
using www.opengis.net;
using System.Threading;
using System.Web;

namespace Kartverket.MetadataEditor.Models.OpenData
{
    internal class OpenMetadataService : IOpenMetadataService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IOpenMetadataFetcher _metadataFetcher;

        private IMetadataService _metadataService;
        IGeoNorge _geoNorge;


        public OpenMetadataService(IMetadataService metadataService, IOpenMetadataFetcher metadataFetcher, IGeoNorge geoNorge)
        {
            _metadataService = metadataService;
            _metadataFetcher = metadataFetcher;
            _geoNorge = geoNorge;
        }

        public async Task<int> SynchronizeMetadata(List<OpenMetadataEndpoint> endpoints)
        {
            Log.Info("Synching open metadata initiated");

            //var endpoints = new List<OpenMetadataEndpoint>();
            //endpoints.Add(new OpenMetadataEndpoint {Url = "https://oslokommune-bym.opendata.arcgis.com/data.json", OrganizationName = "Oslo kommune"});
            //endpoints.Add(new OpenMetadataEndpoint {Url = "http://data-tromso.opendata.arcgis.com/data.json", OrganizationName = "Tromsø kommune"});
            //endpoints.Add(new OpenMetadataEndpoint { Url = "https://hub-frstadkomm.opendata.arcgis.com/data.json", OrganizationName = "Fredrikstad kommune" });

            Log.Info("List of endpoints: ");
            endpoints.ForEach(e => Log.Info(e));

            var numberOfUpdatedMetadata = 0;
            foreach (var endPoint in endpoints)
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

                var distributionWms = GetDistributionWms(metadataModel.DistributionsFormats);

                if (distributionWms != null)
                {
                    string serviceUuidForDataset = GetServiceForDataset(metadataModel.Uuid);

                    if (!string.IsNullOrEmpty(serviceUuidForDataset))
                    {
                        var serviceModel = _metadataService.GetMetadataModel(serviceUuidForDataset);
                        serviceModel.DistributionsFormats = new List<SimpleDistribution>();
                        serviceModel.DistributionsFormats
                            .Add(new SimpleDistribution
                            {
                                Organization = metadataModel.ContactMetadata.Organization,
                                Protocol = "OGC:WMS",
                                FormatName = "png",
                                FormatVersion = "1.0",
                                URL = distributionWms.URL
                            });

                        _metadataService.SaveMetadataModel(serviceModel, SecurityClaim.GetUsername());
                        Log.Info(
                        $"Updated service wms [uuid={serviceUuidForDataset}, title={serviceModel.Title} for metadata entry open dataset: [identifier={identifier}, title={metadataModel.Title}, organization={endpoint.OrganizationName}");

                    }
                    else
                    {
                        dataset.title = dataset.title + " - WMS";
                        string uuid = InsertOpenMetadata(null, dataset, endpoint, "service");
                        var serviceModel = _metadataService.GetMetadataModel(uuid);
                        if(serviceModel != null)
                        {
                        serviceModel.OperatesOn = new List<string>();
                        serviceModel.OperatesOn.Add(identifier);

                            serviceModel.DistributionsFormats = new List<SimpleDistribution>();
                            serviceModel.DistributionsFormats
                                .Add(new SimpleDistribution
                                {
                                    Organization = metadataModel.ContactMetadata.Organization,
                                    Protocol = "OGC:WMS",
                                    FormatName = "png",
                                    FormatVersion = "1.0",
                                    URL = distributionWms.URL
                                });

                            MapDatasetToMetadataViewModel(dataset, serviceModel, endpoint);
                            _metadataService.SaveMetadataModel(serviceModel, SecurityClaim.GetUsername());
                        Log.Info(
                        $"Added service wms [uuid={uuid}, title={serviceModel.Title} for metadata entry open dataset: [identifier={identifier}, title={metadataModel.Title}, organization={endpoint.OrganizationName}");
                    }
                    }
                }

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

        private string GetServiceForDataset(string uuid)
        {
            var filters = new object[]
            {
                        new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                singleChar = "_",
                                wildCard = "%",
                                PropertyName = new PropertyNameType {Text = new[] {"srv:operatesOn"}},
                                Literal = new LiteralType {Text = new[] {uuid}}
                            }
            };

            var filterNames = new ItemsChoiceType23[]
            {
                 ItemsChoiceType23.PropertyIsLike,
            };

            SearchResultsType res = null;

            var tries = 3;
            while (true)
            {
                try
                {
                    res = _geoNorge.SearchWithFilters(filters, filterNames, 1, 200);
                    break; // success!
                }
                catch
                {
                    if (--tries == 0)
                        throw;
                    Thread.Sleep(3000);
                }
            }

            if (res != null && res.numberOfRecordsMatched != "0")
            {
                foreach (var item in res.Items)
                {
                    RecordType record = (RecordType)item;

                    for (int i = 0; i < record.ItemsElementName.Length; i++)
                    {
                        var name = record.ItemsElementName[i];
                        var value = record.Items[i].Text != null ? record.Items[i].Text[0] : null;

                        if (name == ItemsChoiceType24.identifier)
                            return value;
                    }
                }
            }

            return null;
         }

        private SimpleDistribution GetDistributionWms(List<SimpleDistribution> distributionsFormats)
        {
            return distributionsFormats.Where(p => p.Protocol == "OGC:WMS").FirstOrDefault();
        }

        private string InsertOpenMetadata(string identifier, Dataset dataset, OpenMetadataEndpoint openMetadataEndpoint, string type = "dataset")
        {
            var newMetadata = new MetadataCreateViewModel();
            if(!string.IsNullOrEmpty(identifier))
                newMetadata.Uuid = identifier;
            newMetadata.Type = type;
            newMetadata.Title = dataset.title;
            newMetadata.MetadataContactOrganization =
                !string.IsNullOrEmpty(openMetadataEndpoint.OrganizationName)
                    ? openMetadataEndpoint.OrganizationName
                    : dataset.publisher.name;
            newMetadata.MetadataContactName = dataset.contactPoint.fn;
            newMetadata.MetadataContactEmail = dataset.contactPoint.hasEmail.Replace("mailto:", "");
            var uuid = _metadataService.CreateMetadata(newMetadata, SecurityClaim.GetUsername());
            Log.Info("Created open metadata uuid: " + uuid);
            return uuid;
        }


        private void MapDatasetToMetadataViewModel(Dataset dataset, MetadataViewModel model,
            OpenMetadataEndpoint openMetadataEndpoint)
        {
            if (string.IsNullOrEmpty(model.HierarchyLevel))
                model.HierarchyLevel = "dataset";

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

            if(model.IsDataset())
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

            model.MetadataStandard = "ISO19115:Fra openmetadata standard";
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
            var id = identifier.Split('/').Last();
            if (id.Contains("?id="))
            {
                Uri myUri = new Uri(identifier);
                string paramId = HttpUtility.ParseQueryString(myUri.Query).Get("id");
                if (!string.IsNullOrEmpty(paramId))
                    id = paramId;
            }
            return id;
        }

    }
}