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

        public MetadataIndexViewModel GetMyMetadata(string organizationName, int offset = 1, int limit = 20)
        {
            SearchResultsType results = _geoNorge.SearchWithOrganisationName(organizationName, offset, limit);

            var model = new MetadataIndexViewModel();
            var metadata = new List<MetadataItemViewModel>();
            if (results.Items != null)
            {
                foreach (var item in results.Items)
                {
                    RecordType record = (RecordType)item;

                    string title = null;
                    string uuid = null;
                    string organization = null;
                    string type = null;

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
                    }
                    metadata.Add(new MetadataItemViewModel { Title = title, Uuid = uuid, Organization = organization, Type = type });
                }

                model.MetadataItems = metadata.OrderBy(m => m.Title).ToList();
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

                Keywords = Keyword.CreateDictionary(metadata.Keywords),

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

            _geoNorge.MetadataUpdate(metadata.GetMetadata());
        }

    }
}