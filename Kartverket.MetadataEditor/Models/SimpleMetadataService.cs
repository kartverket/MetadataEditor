using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Arkitektum.GIS.Lib.SerializeUtil;
using Kartverket.MetadataEditor.Util;
using www.opengis.net;
using GeoNorgeAPI;

namespace Kartverket.MetadataEditor.Models
{
    public class SimpleMetadataService
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private GeoNorge _geoNorge;

        public SimpleMetadataService(GeoNorge geonorge)
        {
            _geoNorge = geonorge;
        }

        public SimpleMetadataService()
        {
            System.Collections.Specialized.NameValueCollection settings = System.Web.Configuration.WebConfigurationManager.AppSettings;
            string server = settings["GeoNetworkUrl"];
            string username = settings["GeoNetworkUsername"];
            string password = settings["GeoNetworkPassword"];
            _geoNorge = new GeoNorgeAPI.GeoNorge(username, password, server);
            _geoNorge.OnLogEventDebug += new GeoNorgeAPI.LogEventHandlerDebug(LogEventsDebug);
            _geoNorge.OnLogEventError += new GeoNorgeAPI.LogEventHandlerError(LogEventsError);
        }

        private void LogEventsDebug(string log)
        {

            Log.Debug(log);
        }

        private void LogEventsError(string log, Exception ex)
        {
            Log.Error(log, ex);
        }

        public MetadataIndexViewModel GetMyMetadata(string organizationName, int offset, int limit)
        {
            SearchResultsType results = _geoNorge.SearchWithOrganisationName(organizationName, offset, limit, true);

            return ParseSearchResults(offset, limit, results);
        }

        public MetadataIndexViewModel SearchMetadata(string organizationName, string searchString, int offset, int limit)
        {
            SearchResultsType results = null;
            if (!string.IsNullOrWhiteSpace(organizationName))
            {
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    results = _geoNorge.SearchFreeTextWithOrganisationName(searchString, organizationName, offset, limit);
                }
                else
                {
                    results = _geoNorge.SearchWithOrganisationName(organizationName, offset, limit, true);
                }
            }
            else
            {
                results = _geoNorge.Search(searchString, offset, limit, true);
            }
            var model = ParseSearchResults(offset, limit, results);
            model.SearchOrganization = organizationName;
            model.SearchString = searchString;

            return model;
        }


        public MetadataIndexViewModel GetAllMetadata(string searchString, int offset, int limit)
        {
            SearchResultsType results = _geoNorge.Search(searchString, offset, limit, true);
            var model = ParseSearchResults(offset, limit, results);
            model.SearchString = searchString;
            return model;
        }

        private static MetadataIndexViewModel ParseSearchResults(int offset, int limit, SearchResultsType results)
        {
            var model = new MetadataIndexViewModel();
            var metadata = new Dictionary<string, MetadataItemViewModel>();

            if (results.Items != null)
            {
                foreach (var item in results.Items)
                {
                    RecordType record = (RecordType)item;

                    string title = null;
                    string uuid = null;
                    string publisher = null;
                    string creator = null;
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
                            creator = value;
                        else if (name == ItemsChoiceType24.publisher)
                            publisher = value;
                        else if (name == ItemsChoiceType24.type)
                            type = value;
                        else if (name == ItemsChoiceType24.relation)
                            relation = value;
                    }

                    if (!string.IsNullOrWhiteSpace(publisher))
                    {
                        organization = publisher;
                    }
                    else
                    {
                        organization = creator;
                    }

                    var metadataItem = new MetadataItemViewModel
                    {
                        Title = title,
                        Uuid = uuid,
                        Organization = organization,
                        Type = type,
                        Relation = relation,
                        GeoNetworkViewUrl = GeoNetworkUtil.GetViewUrl(uuid),
                        GeoNetworkXmlDownloadUrl = GeoNetworkUtil.GetXmlDownloadUrl(uuid)
                    };

                    if (uuid != null)
                        metadata.Add(uuid, metadataItem);


                }

                model.MetadataItems = metadata.Values.ToList();
                model.Limit = limit;
                model.Offset = offset;
                model.NumberOfRecordsReturned = int.Parse(results.numberOfRecordsReturned);
                model.TotalNumberOfRecords = int.Parse(results.numberOfRecordsMatched);
            }
            return model;
        }

        public SimpleMetadataViewModel GetMetadataModel(string uuid)
        {
            SimpleMetadata metadata = new SimpleMetadata(_geoNorge.GetRecordByUuid(uuid));

            var model = new SimpleMetadataViewModel()
            {
                Uuid = metadata.Uuid,
                Title = metadata.Title,
                HierarchyLevel = metadata.HierarchyLevel,
                ParentIdentifier = metadata.ParentIdentifier,
                Abstract = metadata.Abstract != null ? metadata.Abstract.Replace("...", "") : "",

                ContactMetadata = new Contact(metadata.ContactMetadata, "pointOfContact"),
                ContactPublisher = new Contact(metadata.ContactPublisher, "publisher"),
                ContactOwner = new Contact(metadata.ContactOwner, "owner"),

                BoundingBoxEast = metadata.BoundingBox != null ? metadata.BoundingBox.EastBoundLongitude : null,
                BoundingBoxWest = metadata.BoundingBox != null ? metadata.BoundingBox.WestBoundLongitude : null,
                BoundingBoxNorth = metadata.BoundingBox != null ? metadata.BoundingBox.NorthBoundLatitude : null,
                BoundingBoxSouth = metadata.BoundingBox != null ? metadata.BoundingBox.SouthBoundLatitude : null,

                SupplementalDescription = metadata.SupplementalDescription,
                SpecificUsage = metadata.SpecificUsage,

                DistributionProtocol = metadata.DistributionDetails != null ? metadata.DistributionDetails.Protocol : null,

                MaintenanceFrequency = metadata.MaintenanceFrequency,
                DateUpdated = metadata.DateUpdated,
                DateMetadataUpdated = metadata.DateMetadataUpdated,

                KeywordsPlace = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, SimpleKeyword.TYPE_PLACE, null)),
                KeywordsNationalInitiative = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE)),
                KeywordsNationalTheme = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME)),

            };

            return model;
        }


        private string ConvertCoordinateWithCommaToPoint(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Replace(',', '.');
            }
            return input;
        }

        private Dictionary<string, string> CreateDictionaryOfEnglishKeywords(List<SimpleKeyword> keywords)
        {
            Dictionary<string, string> englishKeywords = new Dictionary<string, string>();
            foreach (var keyword in keywords)
            {
                if (!string.IsNullOrWhiteSpace(keyword.EnglishKeyword))
                {
                    englishKeywords.Add(keyword.GetPrefix() + "_" + keyword.Keyword, keyword.EnglishKeyword);
                }
            }
            return englishKeywords;
        }


        private List<string> CreateListOfKeywords(List<SimpleKeyword> input)
        {
            List<string> output = new List<string>();
            if (input != null)
            {
                foreach (var keyword in input)
                {
                    output.Add(keyword.Keyword);
                }
            }
            return output;
        }

        public void SaveMetadataModel(SimpleMetadataViewModel model, string username)
        {
            SimpleMetadata metadata = new SimpleMetadata(_geoNorge.GetRecordByUuid(model.Uuid));

            UpdateMetadataFromModel(model, metadata);

            _geoNorge.MetadataUpdate(metadata.GetMetadata(), CreateAdditionalHeadersWithUsername(username, model.Published));
        }

        private void UpdateMetadataFromModel(SimpleMetadataViewModel model, SimpleMetadata metadata)
        {
            metadata.Title = model.Title;
            metadata.Abstract = model.Abstract;

            metadata.SupplementalDescription = model.SupplementalDescription;

            metadata.SpecificUsage = !string.IsNullOrWhiteSpace(model.SpecificUsage) ? model.SpecificUsage : " ";

            var contactMetadata = model.ContactMetadata.ToSimpleContact();
           
            metadata.ContactMetadata = contactMetadata;

            var contactPublisher = model.ContactPublisher.ToSimpleContact();
           
            metadata.ContactPublisher = contactPublisher;

            var contactOwner = model.ContactOwner.ToSimpleContact();
           
            metadata.ContactOwner = contactOwner;

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

            if (!string.IsNullOrWhiteSpace(model.MaintenanceFrequency))
                metadata.MaintenanceFrequency = model.MaintenanceFrequency;

            metadata.DateUpdated = model.DateUpdated;

            //Keep if other keywords than in model
            List<SimpleKeyword> keywordsNotInModel = metadata.Keywords.Where(k => k.Type != SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE
                                                        && k.Type != SimpleKeyword.THESAURUS_NATIONAL_THEME
                                                        && k.Type != SimpleKeyword.TYPE_PLACE).ToList();
            //Get keywords in model
            List<SimpleKeyword> keywordsToUpdate = model.GetAllKeywords();

            metadata.Keywords = keywordsNotInModel.Concat(keywordsToUpdate).ToList();

            SetDefaultValuesOnMetadata(metadata);
        }

        private Dictionary<string, string> CreateAdditionalHeadersWithUsername(string username, string published = "")
        {
            Dictionary<string, string> header = new Dictionary<string, string> { { "GeonorgeUsername", username } };

            bool isAdmin = false;
            bool editorRole = false;

            foreach (var c in System.Security.Claims.ClaimsPrincipal.Current.Claims)
            {

                if (c.Type == "organization")
                    header.Add("GeonorgeOrganization", c.Value);
                else if (c.Type == "role")
                {
                    if (c.Value == "nd.metadata_admin")
                    {
                        header.Add("GeonorgeRole", c.Value);
                        isAdmin = true;
                    }
                    else if (c.Value == "nd.metadata_editor")
                    {
                        editorRole = true;
                    }
                }
            }

            if (!isAdmin && editorRole)
                header.Add("GeonorgeRole", "nd.metadata_editor");

            header.Add("published", published);

            return header;
        }



        private void SetDefaultValuesOnMetadata(SimpleMetadata metadata)
        {
            metadata.DateMetadataUpdated = DateTime.Now;
            metadata.MetadataStandard = "ISO19115";
            metadata.MetadataStandardVersion = "2003";
            metadata.MetadataLanguage = "nor";
        }


        internal List<WmsLayerViewModel> CreateMetadataForLayers(string uuid, List<WmsLayerViewModel> layers, string[] keywords, string username)
        {
            SimpleMetadata parentMetadata = new SimpleMetadata(_geoNorge.GetRecordByUuid(uuid));

            List<SimpleKeyword> selectedKeywordsFromParent = CreateListOfKeywords(keywords);

            List<string> layerIdentifiers = new List<string>();
            foreach (WmsLayerViewModel layer in layers)
            {
                try
                {
                    SimpleMetadata simpleLayer = createMetadataForLayer(parentMetadata, selectedKeywordsFromParent, layer);
                    MetadataTransaction transaction = _geoNorge.MetadataInsert(simpleLayer.GetMetadata(), CreateAdditionalHeadersWithUsername(username));
                    if (transaction.Identifiers != null && transaction.Identifiers.Count > 0)
                    {
                        layer.Uuid = transaction.Identifiers[0];
                        layerIdentifiers.Add(layer.Uuid);
                    }
                }
                catch (Exception e)
                {
                    layer.ErrorMessage = e.Message;
                    Log.Error("Error while creating metadata for layer: " + layer.Title, e);
                }
            }

            parentMetadata.OperatesOn = layerIdentifiers;

            _geoNorge.MetadataUpdate(parentMetadata.GetMetadata());

            return layers;
        }


        internal List<WfsLayerViewModel> CreateMetadataForFeature(string uuid, List<WfsLayerViewModel> layers, string[] keywords, string username)
        {
            SimpleMetadata parentMetadata = new SimpleMetadata(_geoNorge.GetRecordByUuid(uuid));

            List<SimpleKeyword> selectedKeywordsFromParent = CreateListOfKeywords(keywords);

            List<string> layerIdentifiers = new List<string>();
            foreach (WfsLayerViewModel layer in layers)
            {
                try
                {
                    SimpleMetadata simpleLayer = createMetadataForFeature(parentMetadata, selectedKeywordsFromParent, layer);
                    MetadataTransaction transaction = _geoNorge.MetadataInsert(simpleLayer.GetMetadata(), CreateAdditionalHeadersWithUsername(username));
                    if (transaction.Identifiers != null && transaction.Identifiers.Count > 0)
                    {
                        layer.Uuid = transaction.Identifiers[0];
                        layerIdentifiers.Add(layer.Uuid);
                    }
                }
                catch (Exception e)
                {
                    layer.ErrorMessage = e.Message;
                    Log.Error("Error while creating metadata for layer: " + layer.Title, e);
                }
            }

            parentMetadata.OperatesOn = layerIdentifiers;

            _geoNorge.MetadataUpdate(parentMetadata.GetMetadata());

            return layers;
        }

        private List<SimpleKeyword> CreateListOfKeywords(string[] selectedKeywords)
        {
            List<SimpleKeyword> keywords = new List<SimpleKeyword>();

            if (selectedKeywords != null)
            {
                foreach (var keyword in selectedKeywords)
                {
                    SimpleKeyword simpleKeyword = null;
                    if (keyword.StartsWith("Theme_"))
                    {
                        simpleKeyword = new SimpleKeyword { Keyword = stripPrefixFromKeyword(keyword), Type = SimpleKeyword.TYPE_THEME };
                    }
                    else if (keyword.StartsWith("Place_"))
                    {
                        simpleKeyword = new SimpleKeyword { Keyword = stripPrefixFromKeyword(keyword), Type = SimpleKeyword.TYPE_PLACE };
                    }
                    else if (keyword.StartsWith("NationalInitiative_"))
                    {
                        simpleKeyword = new SimpleKeyword { Keyword = stripPrefixFromKeyword(keyword), Thesaurus = SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE };
                    }
                    else if (keyword.StartsWith("Inspire_"))
                    {
                        simpleKeyword = new SimpleKeyword { Keyword = stripPrefixFromKeyword(keyword), Thesaurus = SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1 };
                    }
                    else if (keyword.StartsWith("Other_"))
                    {
                        simpleKeyword = new SimpleKeyword { Keyword = stripPrefixFromKeyword(keyword) };
                    }

                    if (simpleKeyword != null)
                        keywords.Add(simpleKeyword);
                }
            }
            return keywords;
        }

        private string stripPrefixFromKeyword(string input)
        {
            int prefixIndexEnd = input.IndexOf("_");
            if (prefixIndexEnd != -1)
            {
                return input.Substring(prefixIndexEnd + 1);
            }
            else
            {
                return input;
            }
        }

        private SimpleMetadata createMetadataForLayer(SimpleMetadata parentMetadata, List<SimpleKeyword> selectedKeywordsFromParent, WmsLayerViewModel layerModel)
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

            simpleLayer.Keywords = selectedKeywordsFromParent;

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
                simpleLayer.Keywords = existingKeywords;
            }

            simpleLayer.DistributionDetails = new SimpleDistributionDetails
            {
                Name = layerModel.Name,
                Protocol = parentMetadata.DistributionDetails.Protocol,
                URL = parentMetadata.DistributionDetails.URL
            };

            if (!string.IsNullOrWhiteSpace(layerModel.BoundingBoxEast))
            {
                string defaultWestBoundLongitude = "-20";
                string defaultEastBoundLongitude = "38";
                string defaultSouthBoundLatitude = "56";
                string defaultNorthBoundLatitude = "90";

                string parentWestBoundLongitude = defaultWestBoundLongitude;
                string parentEastBoundLongitude = defaultEastBoundLongitude;
                string parentSouthBoundLatitude = defaultSouthBoundLatitude;
                string parentNorthBoundLatitude = defaultNorthBoundLatitude;

                if (parentMetadata.BoundingBox != null)
                {
                    parentWestBoundLongitude = parentMetadata.BoundingBox.WestBoundLongitude;
                    parentEastBoundLongitude = parentMetadata.BoundingBox.EastBoundLongitude;
                    parentSouthBoundLatitude = parentMetadata.BoundingBox.SouthBoundLatitude;
                    parentNorthBoundLatitude = parentMetadata.BoundingBox.NorthBoundLatitude;
                }

                string WestBoundLongitude = layerModel.BoundingBoxWest;
                string EastBoundLongitude = layerModel.BoundingBoxEast;
                string SouthBoundLatitude = layerModel.BoundingBoxSouth;
                string NorthBoundLatitude = layerModel.BoundingBoxNorth;

                decimal number;

                if (!Decimal.TryParse(WestBoundLongitude, out number)
                    || !Decimal.TryParse(EastBoundLongitude, out number)
                    || !Decimal.TryParse(SouthBoundLatitude, out number)
                    || !Decimal.TryParse(NorthBoundLatitude, out number)
                    )
                {
                    WestBoundLongitude = parentWestBoundLongitude;
                    EastBoundLongitude = parentEastBoundLongitude;
                    SouthBoundLatitude = parentSouthBoundLatitude;
                    NorthBoundLatitude = parentNorthBoundLatitude;

                    if (!Decimal.TryParse(WestBoundLongitude, out number)
                       || !Decimal.TryParse(EastBoundLongitude, out number)
                       || !Decimal.TryParse(SouthBoundLatitude, out number)
                       || !Decimal.TryParse(NorthBoundLatitude, out number)
                       )
                    {
                        WestBoundLongitude = defaultWestBoundLongitude;
                        EastBoundLongitude = defaultEastBoundLongitude;
                        SouthBoundLatitude = defaultSouthBoundLatitude;
                        NorthBoundLatitude = defaultNorthBoundLatitude;
                    }

                }


                simpleLayer.BoundingBox = new SimpleBoundingBox
                {
                    EastBoundLongitude = EastBoundLongitude,
                    WestBoundLongitude = WestBoundLongitude,
                    NorthBoundLatitude = NorthBoundLatitude,
                    SouthBoundLatitude = SouthBoundLatitude
                };
            }

            if (!string.IsNullOrWhiteSpace(layerModel.EnglishTitle))
            {
                simpleLayer.EnglishTitle = layerModel.EnglishTitle;
            }

            if (!string.IsNullOrWhiteSpace(layerModel.EnglishAbstract))
            {
                simpleLayer.EnglishAbstract = layerModel.EnglishAbstract;
            }

            return simpleLayer;


        }

        private SimpleMetadata createMetadataForFeature(SimpleMetadata parentMetadata, List<SimpleKeyword> selectedKeywordsFromParent, WfsLayerViewModel layerModel)
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

            simpleLayer.Keywords = selectedKeywordsFromParent;

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
                simpleLayer.Keywords = existingKeywords;
            }

            simpleLayer.DistributionDetails = new SimpleDistributionDetails
            {
                Name = layerModel.Name,
                Protocol = parentMetadata.DistributionDetails.Protocol,
                URL = parentMetadata.DistributionDetails.URL
            };

            if (!string.IsNullOrWhiteSpace(layerModel.BoundingBoxEast))
            {
                string defaultWestBoundLongitude = "-20";
                string defaultEastBoundLongitude = "38";
                string defaultSouthBoundLatitude = "56";
                string defaultNorthBoundLatitude = "90";

                string parentWestBoundLongitude = defaultWestBoundLongitude;
                string parentEastBoundLongitude = defaultEastBoundLongitude;
                string parentSouthBoundLatitude = defaultSouthBoundLatitude;
                string parentNorthBoundLatitude = defaultNorthBoundLatitude;

                if (parentMetadata.BoundingBox != null)
                {
                    parentWestBoundLongitude = parentMetadata.BoundingBox.WestBoundLongitude;
                    parentEastBoundLongitude = parentMetadata.BoundingBox.EastBoundLongitude;
                    parentSouthBoundLatitude = parentMetadata.BoundingBox.SouthBoundLatitude;
                    parentNorthBoundLatitude = parentMetadata.BoundingBox.NorthBoundLatitude;
                }

                string WestBoundLongitude = layerModel.BoundingBoxWest;
                string EastBoundLongitude = layerModel.BoundingBoxEast;
                string SouthBoundLatitude = layerModel.BoundingBoxSouth;
                string NorthBoundLatitude = layerModel.BoundingBoxNorth;

                decimal number;

                if (!Decimal.TryParse(WestBoundLongitude, out number)
                    || !Decimal.TryParse(EastBoundLongitude, out number)
                    || !Decimal.TryParse(SouthBoundLatitude, out number)
                    || !Decimal.TryParse(NorthBoundLatitude, out number)
                    )
                {
                    WestBoundLongitude = parentWestBoundLongitude;
                    EastBoundLongitude = parentEastBoundLongitude;
                    SouthBoundLatitude = parentSouthBoundLatitude;
                    NorthBoundLatitude = parentNorthBoundLatitude;

                    if (!Decimal.TryParse(WestBoundLongitude, out number)
                       || !Decimal.TryParse(EastBoundLongitude, out number)
                       || !Decimal.TryParse(SouthBoundLatitude, out number)
                       || !Decimal.TryParse(NorthBoundLatitude, out number)
                       )
                    {
                        WestBoundLongitude = defaultWestBoundLongitude;
                        EastBoundLongitude = defaultEastBoundLongitude;
                        SouthBoundLatitude = defaultSouthBoundLatitude;
                        NorthBoundLatitude = defaultNorthBoundLatitude;
                    }

                }


                simpleLayer.BoundingBox = new SimpleBoundingBox
                {
                    EastBoundLongitude = EastBoundLongitude,
                    WestBoundLongitude = WestBoundLongitude,
                    NorthBoundLatitude = NorthBoundLatitude,
                    SouthBoundLatitude = SouthBoundLatitude
                };
            }

            if (!string.IsNullOrWhiteSpace(layerModel.EnglishTitle))
            {
                simpleLayer.EnglishTitle = layerModel.EnglishTitle;
            }

            if (!string.IsNullOrWhiteSpace(layerModel.EnglishAbstract))
            {
                simpleLayer.EnglishAbstract = layerModel.EnglishAbstract;
            }

            return simpleLayer;


        }


        internal string CreateMetadata(MetadataCreateViewModel model, string username)
        {
            SimpleMetadata metadata = null;
            if (model.Type.Equals("service"))
            {
                metadata = SimpleMetadata.CreateService();
            }
            else
            {
                metadata = SimpleMetadata.CreateDataset();
                if (model.Type.Equals("software"))
                {
                    metadata.HierarchyLevel = "software";
                }
                else if (model.Type.Equals("series"))
                {
                    metadata.HierarchyLevel = "series";
                }
            }

            metadata.Title = model.Title;
            metadata.Abstract = "...";
            metadata.ContactMetadata = new SimpleContact
            {
                Name = model.MetadataContactName,
                Email = model.MetadataContactEmail,
                Organization = model.MetadataContactOrganization,
                Role = "pointOfContact"
            };

            metadata.ContactPublisher = new SimpleContact
            {
                Name = model.MetadataContactName,
                Email = model.MetadataContactEmail,
                Organization = model.MetadataContactOrganization,
                Role = "publisher"
            };
            metadata.ContactOwner = new SimpleContact
            {
                Name = model.MetadataContactName,
                Email = model.MetadataContactEmail,
                Organization = model.MetadataContactOrganization,
                Role = "owner"
            };

            DateTime now = DateTime.Now;
            metadata.DateCreated = now;
            metadata.DatePublished = now;
            metadata.DateUpdated = now;

            SetDefaultValuesOnMetadata(metadata);

            _geoNorge.MetadataInsert(metadata.GetMetadata(), CreateAdditionalHeadersWithUsername(username));

            return metadata.Uuid;
        }



        internal void DeleteMetadata(string uuid, string username)
        {
            _geoNorge.MetadataDelete(uuid, CreateAdditionalHeadersWithUsername(username));
        }

    }
}