using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using www.opengis.net;
using GeoNorgeAPI;

namespace Kartverket.MetadataEditor.Models
{
    public class MetadataService
    {
        private GeoNorge _geoNorge;

        public MetadataService(GeoNorge geonorge)
        {
            _geoNorge = geonorge;
        }

        public MetadataService()
        {
            System.Collections.Specialized.NameValueCollection settings = System.Web.Configuration.WebConfigurationManager.AppSettings;
            string server = settings["GeoNetworkServer"];
            string username = settings["GeoNetworkUsername"];
            string password = settings["GeoNetworkPassword"];
            _geoNorge = new GeoNorgeAPI.GeoNorge(username, password, server);
        }

        public MetadataIndexViewModel GetMyMetadata(string organizationName, int offset, int limit)
        {
            SearchResultsType results = _geoNorge.SearchWithOrganisationName(organizationName, offset, limit);

            var model = new MetadataIndexViewModel();
            var metadata = new Dictionary<string, MetadataItemViewModel>();

            var relations = new Dictionary<string, List<MetadataItemViewModel>>();

            if (results.Items != null)
            {
                foreach (var item in results.Items)
                {
                    RecordType record = (RecordType)item;

                    string title = null;
                    string uuid = null;
                    string organization = null;
                    string type = null;
                    string relation = null;

                    for (int i = 0; i < record.ItemsElementName.Length; i++)
                    {
                        var name = record.ItemsElementName[i];
                        var value = record.Items[i].Text != null ? record.Items[i].Text[0] : null;

                        if (name == ItemsChoiceType24.title)
                            title = value;
                        else if (name == ItemsChoiceType24.identifier)
                            uuid = value;
                        else if (name == ItemsChoiceType24.creator)
                            organization = value;
                        else if (name == ItemsChoiceType24.type)
                            type = value;
                        else if (name == ItemsChoiceType24.relation)
                            relation = value;
                    }

                    var metadataItem = new MetadataItemViewModel { Title = title, Uuid = uuid, Organization = organization, Type = type };
                    if (!string.IsNullOrWhiteSpace(relation))
                    {
                        if (relations.ContainsKey(relation))
                        {
                            relations[relation].Add(metadataItem);
                        }
                        else
                        {
                            relations.Add(relation, new List<MetadataItemViewModel> { metadataItem });
                        }
                    } else {
                        metadata.Add(uuid, metadataItem);
                    }
                }

                foreach (string uuid in relations.Keys)
                {
                    List<MetadataItemViewModel> orderedValues = relations[uuid].OrderBy(m => m.Title).ToList();

                    foreach (MetadataItemViewModel item in orderedValues)
                    {
                        if (metadata.ContainsKey(uuid))
                        {
                            metadata[uuid].Relations.Add(item);
                        }
                        else
                        {
                            metadata.Add(item.Uuid, item);
                        }
                    }
                }

                model.MetadataItems = metadata.Values.OrderBy(m => m.Title).ToList();
                model.Limit = limit;
                model.Offset = offset;
                model.NumberOfRecordsReturned = int.Parse(results.numberOfRecordsReturned);
                model.TotalNumberOfRecords = int.Parse(results.numberOfRecordsMatched);
            }
            return model;
        }


        public MetadataViewModel GetMetadataModel(string uuid)
        {
            SimpleMetadata metadata = new SimpleMetadata(_geoNorge.GetRecordByUuid(uuid));

            var model = new MetadataViewModel() { 
                Uuid = metadata.Uuid,
                Title = metadata.Title,
                HierarchyLevel = metadata.HierarchyLevel,
                Abstract = metadata.Abstract,
                Purpose = metadata.Purpose,

                ContactPointOfContact = new Contact(metadata.ContactPointOfContact, "pointOfContact"),
                ContactPublisher = new Contact(metadata.ContactPublisher, "publisher"),

                KeywordsTheme = SimpleKeyword.Filter(metadata.Keywords, SimpleKeyword.TYPE_THEME, null),
                KeywordsPlace = SimpleKeyword.Filter(metadata.Keywords, SimpleKeyword.TYPE_PLACE, null),
                KeywordsNationalInitiative = SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE),
                KeywordsInspire = SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1),
                KeywordsServiceTaxonomy = SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_SERVICES_TAXONOMY),
                KeywordsOther = SimpleKeyword.Filter(metadata.Keywords, null, null),

                TopicCategory = metadata.TopicCategory,
                SupplementalDescription = metadata.SupplementalDescription,
                
                ProductPageUrl = metadata.ProductPageUrl,
                ProductSheetUrl = metadata.ProductSheetUrl,
                ProductSpecificationUrl = metadata.ProductSpecificationUrl,
                LegendDescriptionUrl = metadata.LegendDescriptionUrl,
                
                Thumbnails = Thumbnail.CreateFromList(metadata.Thumbnails),

                SpatialRepresentation = metadata.SpatialRepresentation,
                DistributionFormatName = metadata.DistributionFormat != null ? metadata.DistributionFormat.Name : null,
                DistributionFormatVersion = metadata.DistributionFormat != null ? metadata.DistributionFormat.Version : null,
                DistributionUrl = metadata.DistributionDetails != null ? metadata.DistributionDetails.URL : null,
                DistributionProtocol = metadata.DistributionDetails != null ? metadata.DistributionDetails.Protocol : null,
                ReferenceSystemCoordinateSystem = metadata.ReferenceSystem != null ? metadata.ReferenceSystem.CoordinateSystem : null,
                ReferenceSystemNamespace = metadata.ReferenceSystem != null ? metadata.ReferenceSystem.Namespace: null,

                QualitySpecificationDate = metadata.QualitySpecification != null ? metadata.QualitySpecification.Date : null,
                QualitySpecificationDateType = metadata.QualitySpecification != null ? metadata.QualitySpecification.DateType : null,
                QualitySpecificationExplanation = metadata.QualitySpecification != null ? metadata.QualitySpecification.Explanation : null,
                QualitySpecificationResult = metadata.QualitySpecification != null ? metadata.QualitySpecification.Result : false,
                QualitySpecificationTitle = metadata.QualitySpecification != null ? metadata.QualitySpecification.Title : null,
                ProcessHistory = metadata.ProcessHistory,
                MaintenanceFrequency = metadata.MaintenanceFrequency,
                ResolutionScale = metadata.ResolutionScale,

                UseLimitations = metadata.Constraints != null ? metadata.Constraints.UseLimitations : null,
                UseConstraints = metadata.Constraints != null ? metadata.Constraints.UseConstraints : null,
                AccessConstraints = metadata.Constraints != null ? metadata.Constraints.AccessConstraints : null,
                SecurityConstraints = metadata.Constraints != null ? metadata.Constraints.SecurityConstraints : null,
                OtherConstraints = metadata.Constraints != null ? metadata.Constraints.OtherConstraints : null,

                DateCreated = metadata.DateCreated,
                DatePublished = metadata.DatePublished,
                DateUpdated = metadata.DateUpdated,
                DateMetadataUpdated = metadata.DateMetadataUpdated,

                Status = metadata.Status,

                BoundingBoxEast = metadata.BoundingBox != null ? metadata.BoundingBox.EastBoundLongitude : null,
                BoundingBoxWest = metadata.BoundingBox != null ? metadata.BoundingBox.WestBoundLongitude : null,
                BoundingBoxNorth = metadata.BoundingBox != null ? metadata.BoundingBox.NorthBoundLatitude : null,
                BoundingBoxSouth = metadata.BoundingBox != null ? metadata.BoundingBox.SouthBoundLatitude : null,
            };

            model.FixThumbnailUrls();
            return model;
        }

        public void SaveMetadataModel(MetadataViewModel model)
        {
            SimpleMetadata metadata = new SimpleMetadata(_geoNorge.GetRecordByUuid(model.Uuid));

            metadata.Title = model.Title;
            metadata.Abstract = model.Abstract;
            metadata.Purpose = model.Purpose;
            metadata.SupplementalDescription = model.SupplementalDescription;
            metadata.ContactPointOfContact = model.ContactPointOfContact.ToSimpleContact();
            metadata.ContactPublisher = model.ContactPublisher.ToSimpleContact();

            // documents
            metadata.ProductSpecificationUrl = model.ProductSpecificationUrl;
            metadata.ProductSheetUrl = model.ProductSheetUrl;
            metadata.ProductPageUrl = model.ProductPageUrl;
            metadata.LegendDescriptionUrl = model.LegendDescriptionUrl;

            // distribution
            metadata.SpatialRepresentation = model.SpatialRepresentation;
            metadata.ReferenceSystem = new SimpleReferenceSystem
            {
                CoordinateSystem = model.ReferenceSystemCoordinateSystem,
                Namespace = model.ReferenceSystemNamespace
            };
            metadata.DistributionFormat = new SimpleDistributionFormat 
            { 
                Name = model.DistributionFormatName,
                Version = model.DistributionFormatVersion
            };
            metadata.DistributionDetails = new SimpleDistributionDetails
            {
                URL = model.DistributionUrl,
                Protocol = model.DistributionProtocol
            };

            // quality
            if (!string.IsNullOrWhiteSpace(model.QualitySpecificationTitle)) {
                metadata.QualitySpecification = new SimpleQualitySpecification
                {
                    Title = model.QualitySpecificationTitle,
                    Date = model.QualitySpecificationDate,
                    DateType = model.QualitySpecificationDateType,
                    Explanation = model.QualitySpecificationExplanation,
                    Result = model.QualitySpecificationResult
                };
            }
            metadata.ProcessHistory = model.ProcessHistory;
            metadata.MaintenanceFrequency = model.MaintenanceFrequency;
            metadata.ResolutionScale = model.ResolutionScale;

            metadata.DateCreated = model.DateCreated;
            metadata.DatePublished = model.DatePublished;
            metadata.DateUpdated = model.DateUpdated;

            if (!string.IsNullOrWhiteSpace(model.BoundingBoxEast))
            {
                metadata.BoundingBox = new SimpleBoundingBox
                {
                    EastBoundLongitude = model.BoundingBoxEast,
                    WestBoundLongitude = model.BoundingBoxWest,
                    NorthBoundLatitude = model.BoundingBoxNorth,
                    SouthBoundLatitude = model.BoundingBoxSouth
                };
            }

            metadata.Constraints = new SimpleConstraints
            {
                AccessConstraints = model.AccessConstraints,
                OtherConstraints = model.OtherConstraints,
                SecurityConstraints = model.SecurityConstraints,
                UseConstraints = model.UseConstraints,
                UseLimitations = model.UseLimitations
            };

            metadata.Keywords = model.GetAllKeywords();

            // hardcoding values
            metadata.DateMetadataUpdated = DateTime.Now;
            metadata.MetadataStandard = "ISO19115";
            metadata.MetadataStandardVersion = "2003";
            metadata.MetadataLanguage = "nor";

            _geoNorge.MetadataUpdate(metadata.GetMetadata());
        }


        internal List<WmsLayerViewModel> CreateMetadataForLayers(string uuid, List<WmsLayerViewModel> layers)
        {

            SimpleMetadata parentMetadata = new SimpleMetadata(_geoNorge.GetRecordByUuid(uuid));

            List<string> layerIdentifiers = new List<string>();
            foreach(WmsLayerViewModel layer in layers) {
                createDuplicateOfMetadata(parentMetadata, layer);
                layerIdentifiers.Add(layer.Uuid);
            }

            parentMetadata.OperatesOn = layerIdentifiers;

            _geoNorge.MetadataUpdate(parentMetadata.GetMetadata());

            return layers;
        }

        private void createDuplicateOfMetadata(SimpleMetadata parentMetadata, WmsLayerViewModel layerModel)
        {
            MD_Metadata_Type parent = parentMetadata.GetMetadata();

            MD_Metadata_Type layer = parent.Copy();
            layer.parentIdentifier = new CharacterString_PropertyType { CharacterString = parent.fileIdentifier.CharacterString };
            layer.fileIdentifier = new CharacterString_PropertyType { CharacterString = Guid.NewGuid().ToString() };

            SimpleMetadata simpleLayer = new SimpleMetadata(layer);

            string title = layerModel.Title;
            if (string.IsNullOrWhiteSpace(title))
            {
                title = layerModel.Name;
            }

            simpleLayer.Title = title;
            
            if (!string.IsNullOrWhiteSpace(layerModel.Abstract))
            {
                simpleLayer.Abstract = layerModel.Abstract;
            }

            if (layerModel.Keywords.Count > 0)
            {
                var existingKeywords = simpleLayer.Keywords;
                foreach (var keyword in layerModel.Keywords)
                {
                    existingKeywords.Add(new SimpleKeyword
                    {
                        Keyword = keyword
                    });
                }
            }

            simpleLayer.DistributionDetails = new SimpleDistributionDetails
            {
                Name = layerModel.Name
            };
            
            if (!string.IsNullOrWhiteSpace(layerModel.BoundingBoxEast))
            {
                simpleLayer.BoundingBox = new SimpleBoundingBox
                {
                    EastBoundLongitude = layerModel.BoundingBoxEast,
                    WestBoundLongitude = layerModel.BoundingBoxWest,
                    NorthBoundLatitude = layerModel.BoundingBoxNorth,
                    SouthBoundLatitude = layerModel.BoundingBoxSouth
                };
            }
            


            MetadataTransaction transaction = _geoNorge.MetadataInsert(layer);
            if (transaction.Identifiers != null && transaction.Identifiers.Count > 0)
            {
                layerModel.Uuid = transaction.Identifiers[0];
            }
        }
    }
}