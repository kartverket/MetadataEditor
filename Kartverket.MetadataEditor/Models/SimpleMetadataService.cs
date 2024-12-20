﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
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
                MetadataStandard = metadata.MetadataStandard,
                Abstract = metadata.Abstract != null ? metadata.Abstract.Replace("...", "") : "",

                ContactMetadata = new Contact(metadata.ContactMetadata, "pointOfContact"),
                ContactPublisher = new Contact(metadata.ContactPublisher, "publisher"),
                ContactOwner = new Contact(metadata.ContactOwner, "owner"),

                SupplementalDescription = metadata.SupplementalDescription,
                SpecificUsage = metadata.SpecificUsage,

                ProcessHistory = metadata.ProcessHistory,
                ProductPageUrl = metadata.ProductPageUrl,

                DistributionUrl = metadata.DistributionDetails != null ? metadata.DistributionDetails.URL : null,
                DistributionProtocol = metadata.DistributionDetails != null ? metadata.DistributionDetails.Protocol : null,

                MaintenanceFrequency = metadata.MaintenanceFrequency,
                DateUpdated = metadata.DateUpdated,
                DateMetadataUpdated = metadata.DateMetadataUpdated,

                KeywordsPlace = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, SimpleKeyword.TYPE_PLACE, null)),
                KeywordsNationalInitiative = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE)),
                KeywordsNationalTheme = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME)),
                KeywordsEnglish = CreateDictionaryOfEnglishKeywords(metadata.Keywords),

                UseConstraints = metadata.Constraints != null ? metadata.Constraints.UseConstraints : null,
                OtherConstraintsLink = metadata.Constraints != null ? metadata.Constraints.OtherConstraintsLink : null,
                OtherConstraintsLinkText = metadata.Constraints != null ? metadata.Constraints.OtherConstraintsLinkText : null,
                OtherConstraintsAccess = metadata.Constraints != null ? metadata.Constraints.OtherConstraintsAccess : null,

                EnglishTitle = metadata.EnglishTitle,
                EnglishAbstract = metadata.EnglishAbstract,
                EnglishContactMetadataOrganization = metadata.ContactMetadata != null ? metadata.ContactMetadata.OrganizationEnglish : null,
                EnglishContactPublisherOrganization = metadata.ContactPublisher != null ? metadata.ContactPublisher.OrganizationEnglish : null,
                EnglishContactOwnerOrganization = metadata.ContactOwner != null ? metadata.ContactOwner.OrganizationEnglish : null

            };

            if (metadata.BoundingBox != null)
            {
                model.BoundingBoxEast = ConvertCoordinateWithCommaToPoint(metadata.BoundingBox.EastBoundLongitude);
                model.BoundingBoxWest = ConvertCoordinateWithCommaToPoint(metadata.BoundingBox.WestBoundLongitude);
                model.BoundingBoxNorth = ConvertCoordinateWithCommaToPoint(metadata.BoundingBox.NorthBoundLatitude);
                model.BoundingBoxSouth = ConvertCoordinateWithCommaToPoint(metadata.BoundingBox.SouthBoundLatitude);
            }

            // Translations
            model.TitleFromSelectedLanguage = model.TitleTranslated();
            model.LegendDescriptionUrl = metadata.LegendDescriptionUrl;
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

            _geoNorge.MetadataUpdate(metadata.GetMetadata(), GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username, model.Published));
        }

        private void UpdateMetadataFromModel(SimpleMetadataViewModel model, SimpleMetadata metadata)
        {

            metadata.ParentIdentifier = model.ParentIdentifier;

            metadata.Title = model.Title;
            metadata.Abstract = model.Abstract;

            metadata.SupplementalDescription = model.SupplementalDescription;

            metadata.SpecificUsage = !string.IsNullOrWhiteSpace(model.SpecificUsage) ? model.SpecificUsage : " ";

            metadata.ProcessHistory = !string.IsNullOrWhiteSpace(model.ProcessHistory) ? model.ProcessHistory : " ";

            metadata.ProductPageUrl = model.ProductPageUrl;

            var contactMetadata = model.ContactMetadata.ToSimpleContact();
            if (!string.IsNullOrWhiteSpace(model.EnglishContactMetadataOrganization))
            {
                contactMetadata.OrganizationEnglish = model.EnglishContactMetadataOrganization;
            }

            metadata.ContactMetadata = contactMetadata;

            var contactPublisher = model.ContactPublisher.ToSimpleContact();
            if (!string.IsNullOrWhiteSpace(model.EnglishContactPublisherOrganization))
            {
                contactPublisher.OrganizationEnglish = model.EnglishContactPublisherOrganization;
            }

            metadata.ContactPublisher = contactPublisher;

            var contactOwner = model.ContactOwner.ToSimpleContact();
            if (!string.IsNullOrWhiteSpace(model.EnglishContactOwnerOrganization))
            {
                contactOwner.OrganizationEnglish = model.EnglishContactOwnerOrganization;
            }

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
            List<SimpleKeyword> keywordsNotInModel = metadata.Keywords.Where(k => k.Thesaurus != SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE
                                                        && k.Thesaurus != SimpleKeyword.THESAURUS_NATIONAL_THEME
                                                        && k.Type != SimpleKeyword.TYPE_PLACE).ToList();
            //Get keywords in model
            List<SimpleKeyword> keywordsToUpdate = model.GetAllKeywords();

            bool hasEnglishFields = false;
            // don't create PT_FreeText fields if it isn't necessary
            if (!string.IsNullOrWhiteSpace(model.EnglishTitle))
            {
                metadata.EnglishTitle = model.EnglishTitle;
                hasEnglishFields = true;
            }
            if (!string.IsNullOrWhiteSpace(model.EnglishAbstract))
            {
                metadata.EnglishAbstract = model.EnglishAbstract;
                hasEnglishFields = true;
            }

            if (hasEnglishFields)
                metadata.SetLocale(SimpleMetadata.LOCALE_ENG);


            metadata.Keywords = keywordsNotInModel.Concat(keywordsToUpdate).ToList();


                metadata.DistributionDetails = new SimpleDistributionDetails
                {
                    URL = model.DistributionUrl,
                    Protocol = model.DistributionProtocol
                };

            var accessConstraintsSelected = model.OtherConstraintsAccess;
            string otherConstraintsAccess = model.OtherConstraintsAccess;

            var accessConstraintsLink = "http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/noLimitations";
            if (string.IsNullOrEmpty(accessConstraintsSelected))
                accessConstraintsLink = null;

            Dictionary<string, string> inspireAccessRestrictions = GetInspireAccessRestrictions();

            if (!string.IsNullOrEmpty(accessConstraintsSelected))
            {
                if (accessConstraintsSelected.ToLower() == "no restrictions" || accessConstraintsSelected.ToLower() == "norway digital restricted")
                {
                    otherConstraintsAccess = accessConstraintsSelected;

                    if (accessConstraintsSelected.ToLower() == "no restrictions")
                        accessConstraintsSelected = inspireAccessRestrictions[accessConstraintsLink];

                    if (accessConstraintsSelected.ToLower() == "norway digital restricted")
                    {
                        accessConstraintsLink = "http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1d";
                        accessConstraintsSelected = inspireAccessRestrictions[accessConstraintsLink];
                    }

                }
                else if (accessConstraintsSelected == "restricted")
                {
                    otherConstraintsAccess = null;
                    accessConstraintsLink = "http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1b";
                    accessConstraintsSelected = inspireAccessRestrictions[accessConstraintsLink];
                }
            }

            //if (string.IsNullOrEmpty(model.UseConstraints))
            //{
            //    model.OtherConstraintsLink = "http://inspire.ec.europa.eu/metadata-codelist/ConditionsApplyingToAccessAndUse/noConditionsApply";
            //    model.OtherConstraintsLinkText = "No conditions apply to access and use";
            //}

            metadata.Constraints = new SimpleConstraints
            {
                AccessConstraints = !string.IsNullOrWhiteSpace(accessConstraintsSelected) ? accessConstraintsSelected : "",
                AccessConstraintsLink = accessConstraintsLink,
                //OtherConstraintsLink = !string.IsNullOrWhiteSpace(model.OtherConstraintsLink) ? model.OtherConstraintsLink : null,
                UseConstraintsLicenseLink = !string.IsNullOrWhiteSpace(model.OtherConstraintsLink) ? model.OtherConstraintsLink : null,
                //OtherConstraintsLinkText = !string.IsNullOrWhiteSpace(model.OtherConstraintsLinkText) ? model.OtherConstraintsLinkText : null,
                UseConstraintsLicenseLinkText = !string.IsNullOrWhiteSpace(model.OtherConstraintsLinkText) ? model.OtherConstraintsLinkText : null,
               
                //UseConstraints = !string.IsNullOrWhiteSpace(model.UseConstraints) ? "license" : "",
                //UseLimitations = !string.IsNullOrWhiteSpace(model.UseLimitations) ? model.UseLimitations : "",
                //OtherConstraintsAccess = !string.IsNullOrWhiteSpace(otherConstraintsAccess) ? otherConstraintsAccess : "",
            };

            metadata.LegendDescriptionUrl = model.LegendDescriptionUrl;

            SetDefaultValuesOnMetadata(metadata);
        }


        public Dictionary<string, string> GetInspireAccessRestrictions(string culture = "no")
        {
            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            c.Headers.Remove("Accept-Language");
            c.Headers.Add("Accept-Language", culture);
            var data = c.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/metadata-kodelister/inspire-tilgangsrestriksjoner");
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            Dictionary<string, string> inspire = new Dictionary<string, string>();

            var items = response["containeditems"];

            foreach (var item in items)
            {
                var id = item["codevalue"].ToString();
                id = id.Replace("https", "http");
                string label = item["label"].ToString();
                string status = item["status"].ToString();



                if (status == "Gyldig" || status == "Valid")
                {
                    inspire.Add(id, label);
                }
            }

            return inspire;
        }


        private void SetDefaultValuesOnMetadata(SimpleMetadata metadata)
        {
            metadata.DateMetadataUpdated = DateTime.Now;
            metadata.MetadataStandard = "ISO19115:Norsk versjon";
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
                    MetadataTransaction transaction = _geoNorge.MetadataInsert(simpleLayer.GetMetadata(), GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username));
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
                    MetadataTransaction transaction = _geoNorge.MetadataInsert(simpleLayer.GetMetadata(), GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username));
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
                    else if (keyword.StartsWith("Concept_"))
                    {
                        simpleKeyword = new SimpleKeyword { Keyword = stripPrefixFromKeyword(keyword), Type = SimpleKeyword.THESAURUS_CONCEPT };
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

            _geoNorge.MetadataInsert(metadata.GetMetadata(), GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username));

            return metadata.Uuid;
        }



        internal void DeleteMetadata(string uuid, string username)
        {
            _geoNorge.MetadataDelete(uuid, GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username));
        }

    }
}