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
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Kartverket.MetadataEditor.Models
{
    public class MetadataService
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private GeoNorge _geoNorge;

        public MetadataService(GeoNorge geonorge)
        {
            _geoNorge = geonorge;
        }

        public MetadataService()
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
                    results = _geoNorge.SearchFreeTextWithOrganisationMetadataPointOfContact(searchString, organizationName, offset, limit);
                }
                else
                {
                    results = _geoNorge.SearchWithOrganisationMetadataPointOfContact(organizationName, offset, limit, true);
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

                    string uri = null;
                    string uriProtocol = null;
                    string uriName = null;

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
                        else if (name == ItemsChoiceType24.URI)
                        {
                            uri = value;
                            var uriAttributes = (SimpleUriLiteral)record.Items[i];
                            if (uriAttributes != null)
                            {
                                if (!string.IsNullOrEmpty(uriAttributes.protocol))
                                    uriProtocol = uriAttributes.protocol;
                                if (!string.IsNullOrEmpty(uriAttributes.name))
                                    uriName = uriAttributes.name;
                            }
                        }
                    } 

                    if (!string.IsNullOrWhiteSpace(creator))
                    {
                        organization = creator;
                    }
                    else
                    {
                        organization = publisher;
                    }

                    var metadataItem = new MetadataItemViewModel
                    {
                        Title = title,
                        Uuid = uuid,
                        Organization = organization,
                        Type = type,
                        Relation = relation,
                        GeoNetworkViewUrl = GeoNetworkUtil.GetViewUrl(uuid),
                        GeoNetworkXmlDownloadUrl = GeoNetworkUtil.GetXmlDownloadUrl(uuid),
                        Uri = uri,
                        UriProtocol = uriProtocol,
                        UriName = uriName
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

        public MetadataViewModel GetMetadataModel(string uuid)
        {
            SimpleMetadata metadata = new SimpleMetadata(_geoNorge.GetRecordByUuid(uuid));

            var model = new MetadataViewModel()
            {
                Uuid = metadata.Uuid,
                Title = metadata.Title,
                HierarchyLevel = metadata.HierarchyLevel,
                ParentIdentifier = metadata.ParentIdentifier,
                MetadataStandard = metadata.MetadataStandard,
                Abstract = metadata.Abstract != null ? metadata.Abstract.Replace("...", "") : "",
                Purpose = metadata.Purpose,

                ContactMetadata = new Contact(metadata.ContactMetadata, "pointOfContact"),
                ContactPublisher = new Contact(metadata.ContactPublisher, "publisher"),
                ContactOwner = new Contact(metadata.ContactOwner, "owner"),

                KeywordsTheme = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, SimpleKeyword.TYPE_THEME, null)),
                KeywordsPlace = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, SimpleKeyword.TYPE_PLACE, null)),
                KeywordsNationalInitiative = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE)),
                KeywordsNationalTheme = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME)),
                KeywordsConcept = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_CONCEPT)),
                KeywordsInspire = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1)),
                KeywordsServiceTaxonomy = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_SERVICES_TAXONOMY)),
                KeywordsServiceType = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_SERVICE_TYPE)),
                KeywordsOther = CreateListOfKeywords(SimpleKeyword.Filter(metadata.Keywords, null, null)),
                KeywordsEnglish = CreateDictionaryOfEnglishKeywords(metadata.Keywords),

                TopicCategory = metadata.TopicCategory,
                SupplementalDescription = metadata.SupplementalDescription,
                SpecificUsage = metadata.SpecificUsage,

                ProductPageUrl = metadata.ProductPageUrl,
                ProductSheetUrl = metadata.ProductSheetUrl,
                ProductSpecificationUrl = metadata.ProductSpecificationUrl,
                ApplicationSchema = metadata.ApplicationSchema,
                LegendDescriptionUrl = metadata.LegendDescriptionUrl,
                CoverageUrl = metadata.CoverageUrl,
                HelpUrl = metadata.HelpUrl,

                Thumbnails = Thumbnail.CreateFromList(metadata.Thumbnails),

                SpatialRepresentation = metadata.SpatialRepresentation,
                DistributionFormatName = metadata.DistributionFormat != null ? metadata.DistributionFormat.Name : null,
                DistributionFormatVersion = metadata.DistributionFormat != null ? metadata.DistributionFormat.Version : null,
                DistributionFormats = metadata.DistributionFormats != null ? metadata.DistributionFormats : new List<SimpleDistributionFormat> { new SimpleDistributionFormat() },
                DistributionsFormats = metadata.DistributionsFormats != null ? metadata.DistributionsFormats : new List<SimpleDistribution> { new SimpleDistribution() },
                FormatDistributions = GetFormatDistributions(metadata.DistributionsFormats),
                DistributionUrl = metadata.DistributionDetails != null ? metadata.DistributionDetails.URL : null,
                DistributionProtocol = metadata.DistributionDetails != null ? metadata.DistributionDetails.Protocol : null,
                DistributionName = metadata.DistributionDetails != null ? metadata.DistributionDetails.Name : null,
                UnitsOfDistribution = metadata.DistributionDetails != null ? metadata.DistributionDetails.UnitsOfDistribution : null,

                ReferenceSystemCoordinateSystem = metadata.ReferenceSystem != null ? metadata.ReferenceSystem.CoordinateSystem : null,
                ReferenceSystemNamespace = metadata.ReferenceSystem != null ? metadata.ReferenceSystem.Namespace : null,
                ReferenceSystems = metadata.ReferenceSystems != null && metadata.ReferenceSystems.Count == 0 ? null : metadata.ReferenceSystems,

                //QualitySpecificationDate = (metadata.QualitySpecification != null && !string.IsNullOrWhiteSpace(metadata.QualitySpecification.Date)) ? DateTime.Parse(metadata.QualitySpecification.Date) : (DateTime?)null,
                //QualitySpecificationDateType = metadata.QualitySpecification != null ? metadata.QualitySpecification.DateType : null,
                //QualitySpecificationExplanation = metadata.QualitySpecification != null ? metadata.QualitySpecification.Explanation : null,
                //QualitySpecificationResult = metadata.QualitySpecification != null ? metadata.QualitySpecification.Result : false,
                //QualitySpecificationTitle = metadata.QualitySpecification != null ? metadata.QualitySpecification.Title : null,
                ProcessHistory = metadata.ProcessHistory,
                MaintenanceFrequency = metadata.MaintenanceFrequency,
                ResolutionScale = metadata.ResolutionScale,

                UseLimitations = metadata.Constraints != null ? metadata.Constraints.UseLimitations : null,
                EnglishUseLimitations = metadata.Constraints != null ? metadata.Constraints.EnglishUseLimitations : null,
                UseConstraints = metadata.Constraints != null ? metadata.Constraints.UseConstraints : null,
                AccessConstraints = metadata.Constraints != null ? metadata.Constraints.AccessConstraints : null,
                SecurityConstraints = metadata.Constraints != null ? metadata.Constraints.SecurityConstraints : null,
                SecurityConstraintsNote = metadata.Constraints != null ? metadata.Constraints.SecurityConstraintsNote : null,
                OtherConstraints = metadata.Constraints != null ? metadata.Constraints.OtherConstraints : null,
                EnglishOtherConstraints = metadata.Constraints != null ? metadata.Constraints.EnglishOtherConstraints : null,
                OtherConstraintsLink = metadata.Constraints != null ? metadata.Constraints.OtherConstraintsLink : null,
                OtherConstraintsLinkText = metadata.Constraints != null ? metadata.Constraints.OtherConstraintsLinkText : null,
                OtherConstraintsAccess = metadata.Constraints != null ? metadata.Constraints.OtherConstraintsAccess : null,

                DateCreated = metadata.DateCreated,
                DatePublished = metadata.DatePublished,
                DateUpdated = metadata.DateUpdated,
                DateMetadataUpdated = metadata.DateMetadataUpdated,
                DateMetadataValidFrom = string.IsNullOrEmpty(metadata.ValidTimePeriod.ValidFrom) ? (DateTime?)null : DateTime.Parse(metadata.ValidTimePeriod.ValidFrom),
                DateMetadataValidTo = string.IsNullOrEmpty(metadata.ValidTimePeriod.ValidTo) ? (DateTime?)null : DateTime.Parse(metadata.ValidTimePeriod.ValidTo),

                Status = metadata.Status,
                OrderingInstructions = (metadata.AccessProperties != null && !string.IsNullOrEmpty(metadata.AccessProperties.OrderingInstructions)) ? metadata.AccessProperties.OrderingInstructions : "",

                BoundingBoxEast = metadata.BoundingBox != null ? metadata.BoundingBox.EastBoundLongitude : null,
                BoundingBoxWest = metadata.BoundingBox != null ? metadata.BoundingBox.WestBoundLongitude : null,
                BoundingBoxNorth = metadata.BoundingBox != null ? metadata.BoundingBox.NorthBoundLatitude : null,
                BoundingBoxSouth = metadata.BoundingBox != null ? metadata.BoundingBox.SouthBoundLatitude : null,

                EnglishTitle = metadata.EnglishTitle,
                EnglishAbstract = metadata.EnglishAbstract,
                EnglishPurpose = metadata.EnglishPurpose,
                EnglishSupplementalDescription = metadata.EnglishSupplementalDescription,
                EnglishSpecificUsage = metadata.EnglishSpecificUsage,
                EnglishProcessHistory = metadata.EnglishProcessHistory,
                EnglishContactMetadataOrganization = metadata.ContactMetadata != null ? metadata.ContactMetadata.OrganizationEnglish : null,
                EnglishContactPublisherOrganization = metadata.ContactPublisher != null ? metadata.ContactPublisher.OrganizationEnglish : null,
                EnglishContactOwnerOrganization = metadata.ContactOwner != null ? metadata.ContactOwner.OrganizationEnglish : null,
            };

            if (metadata.BoundingBox != null)
            {
                model.BoundingBoxEast = ConvertCoordinateWithCommaToPoint(metadata.BoundingBox.EastBoundLongitude);
                model.BoundingBoxWest = ConvertCoordinateWithCommaToPoint(metadata.BoundingBox.WestBoundLongitude);
                model.BoundingBoxNorth = ConvertCoordinateWithCommaToPoint(metadata.BoundingBox.NorthBoundLatitude);
                model.BoundingBoxSouth = ConvertCoordinateWithCommaToPoint(metadata.BoundingBox.SouthBoundLatitude);
            }

            if (metadata.ProductSpecificationOther != null) 
            {
                model.ProductSpecificationOther = new SimpleOnlineResource
                {
                    Name = metadata.ProductSpecificationOther.Name,
                    URL = metadata.ProductSpecificationOther.URL
                };
            }

            model.FixThumbnailUrls();

            model.OperatesOn = metadata.OperatesOn !=null ? metadata.OperatesOn : new List<string>();
            model.CrossReference = metadata.CrossReference != null ? metadata.CrossReference : new List<string>();

            if (metadata.ResourceReference != null)
            {
                model.ResourceReferenceCode = metadata.ResourceReference.Code != null ? metadata.ResourceReference.Code : null;
                model.ResourceReferenceCodespace = metadata.ResourceReference.Codespace != null ? metadata.ResourceReference.Codespace : null;
            }

            getQualitySpecifications(model, metadata);

            // Translations
            model.TitleFromSelectedLanguage = model.NameTranslated();

            return model;
        }

        public Dictionary<DistributionGroup, Distribution> GetFormatDistributions(List<SimpleDistribution> distributions)
        {
            Dictionary<DistributionGroup, Distribution> formatDistributions = new Dictionary<DistributionGroup, Distribution>();
            if (distributions != null && distributions.Count > 0)
            {
                foreach (var distributionFormat in distributions)
                {
                    DistributionGroup group = new DistributionGroup();
                    group.Organization = distributionFormat.Organization;
                    group.Protocol = distributionFormat.Protocol;
                    group.Url = distributionFormat.URL;

                    SimpleDistribution details = new SimpleDistribution();
                    details.Organization = distributionFormat.Organization;
                    details.Protocol = distributionFormat.Protocol;
                    details.URL = distributionFormat.URL;
                    details.UnitsOfDistribution = distributionFormat.UnitsOfDistribution;
                    details.Name = distributionFormat.Name;


                    SimpleDistributionFormat format = new SimpleDistributionFormat();
                    format.Name = distributionFormat.FormatName;
                    format.Version = distributionFormat.FormatVersion;


                    if (!formatDistributions.ContainsKey(group))
                    {
                        List<SimpleDistributionFormat> formatList = new List<SimpleDistributionFormat>();
                        formatList.Add(format);
                        formatDistributions.Add(group, new Distribution
                        {
                            Details = details,
                            Formats = formatList

                        });
                    }
                    else
                    {
                        Distribution distro = formatDistributions[group];
                        distro.Formats.Add(format);
                        formatDistributions[group] = distro;
                    }

                }
            }

            if (formatDistributions.Count < 1)
            {
                List<SimpleDistributionFormat> formatList = new List<SimpleDistributionFormat>();
                formatList.Add(new SimpleDistributionFormat { Name = "", Version="" });
                formatDistributions.Add(new DistributionGroup { Organization="", Protocol="", Url="" }, new Distribution
                {
                    Details = new SimpleDistribution(),
                    Formats = formatList

                });
            }

            return formatDistributions;
        }

        private void getQualitySpecifications(MetadataViewModel model, SimpleMetadata metadata)
        {
            if (metadata.QualitySpecifications != null && metadata.QualitySpecifications.Count > 0)
            {
                foreach (var qualitySpecification in metadata.QualitySpecifications)
                {
                    string responsible = !string.IsNullOrEmpty(qualitySpecification.Responsible) ? qualitySpecification.Responsible : "";
                    responsible = responsible.ToLower();

                    string title = qualitySpecification.Title != null ? qualitySpecification.Title : "";
                    title = title.ToLower();

                    if (title.Contains("commission regulation") || title.Contains("Inspire"))
                        responsible = "inspire";
                    else if (title.Contains("sosi") && title != "sosi applikasjonsskjema")
                        responsible = "sosi";

                    if (responsible == "inspire")
                    {
                        model.QualitySpecificationDateInspire = (!string.IsNullOrWhiteSpace(qualitySpecification.Date)) ? DateTime.Parse(qualitySpecification.Date) : (DateTime?)null;
                        model.QualitySpecificationDateTypeInspire = (!string.IsNullOrWhiteSpace(qualitySpecification.DateType)) ? qualitySpecification.DateType : null;
                        model.QualitySpecificationExplanationInspire = qualitySpecification.Explanation != null ? qualitySpecification.Explanation : null;
                        model.EnglishQualitySpecificationExplanationInspire = qualitySpecification.Explanation != null ? qualitySpecification.EnglishExplanation : null;
                        model.QualitySpecificationResultInspire = qualitySpecification.Result;
                        model.QualitySpecificationTitleInspire = qualitySpecification.Title != null ? qualitySpecification.Title : null;

                    }
                    else if (responsible == "sosi")
                    {
                        model.QualitySpecificationDateSosi = (!string.IsNullOrWhiteSpace(qualitySpecification.Date)) ? DateTime.Parse(qualitySpecification.Date) : (DateTime?)null;
                        model.QualitySpecificationDateTypeSosi = (!string.IsNullOrWhiteSpace(qualitySpecification.DateType)) ? qualitySpecification.DateType : null;
                        model.QualitySpecificationExplanationSosi = qualitySpecification.Explanation != null ? qualitySpecification.Explanation : null;
                        model.EnglishQualitySpecificationExplanationSosi = qualitySpecification.Explanation != null ? qualitySpecification.EnglishExplanation : null;
                        model.QualitySpecificationResultSosi = qualitySpecification.Result;
                        model.QualitySpecificationTitleSosi = qualitySpecification.Title != null ? qualitySpecification.Title : null;
                    }
                    else if (responsible == "uml-sosi")
                    {
                        model.QualitySpecificationResultSosiConformApplicationSchema = qualitySpecification.Result;
                    }
                    else if (responsible == "uml-gml")
                    {
                        model.QualitySpecificationResultSosiConformGmlApplicationSchema = qualitySpecification.Result;
                    }
                    else
                    {
                        model.QualitySpecificationDate = (!string.IsNullOrWhiteSpace(qualitySpecification.Date)) ? DateTime.Parse(qualitySpecification.Date) : (DateTime?)null;
                        model.QualitySpecificationDateType = (!string.IsNullOrWhiteSpace(qualitySpecification.DateType)) ? qualitySpecification.DateType : null;
                        model.QualitySpecificationExplanation = qualitySpecification.Explanation != null ? qualitySpecification.Explanation : null;
                        model.EnglishQualitySpecificationExplanation = qualitySpecification.Explanation != null ? qualitySpecification.EnglishExplanation : null;
                        model.QualitySpecificationResult = qualitySpecification.Result;
                        model.QualitySpecificationTitle = qualitySpecification.Title != null ? qualitySpecification.Title : null;
                    }
                }

            }
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
                    if(!string.IsNullOrEmpty(keyword.KeywordLink))
                        output.Add(keyword.Keyword + "|" + keyword.KeywordLink);
                    else
                    output.Add(keyword.Keyword);
                }
            }
            return output;
        }

        public void SaveMetadataModel(MetadataViewModel model, string username)
        {
            SimpleMetadata metadata = new SimpleMetadata(_geoNorge.GetRecordByUuid(model.Uuid));

            UpdateMetadataFromModel(model, metadata);

            var transaction = _geoNorge.MetadataUpdate(metadata.GetMetadata(), CreateAdditionalHeadersWithUsername(username, model.Published));
            if (transaction.TotalUpdated == "0")
                throw new Exception("Kunne ikke lagre endringene - kontakt systemansvarlig");

            Task.Run(() => ReIndexOperatesOn(model));
            Task.Run(() => RemoveCache(model));
        }

        private void ReIndexOperatesOn(MetadataViewModel metadata)
        {
            if (metadata.OperatesOn != null)
            {
                System.Collections.Specialized.NameValueCollection settings = System.Web.Configuration.WebConfigurationManager.AppSettings;
                string username = settings["KartkatalogUsername"];
                string password = settings["KartkatalogPassword"];

                foreach (var uuid in metadata.OperatesOn)
                {
                    string url = System.Web.Configuration.WebConfigurationManager.AppSettings["KartkatalogUrl"] + "api/metadataupdated";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = WebRequestMethods.Http.Post;
                    request.ContentType = "application/x-www-form-urlencoded";
                    using (var writer = new StreamWriter(request.GetRequestStream()))
                    {
                        writer.Write("uuid="+ uuid);
                        writer.Write("&action=post");
                    }
                    NetworkCredential networkCredential = new NetworkCredential(username, password);
                    CredentialCache myCredentialCache = new CredentialCache { { new Uri(url), "Basic", networkCredential } };
                    request.PreAuthenticate = true;
                    request.Credentials = myCredentialCache;
                    HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                }

                foreach (var uuid in metadata.CrossReference)
                {
                    string url = System.Web.Configuration.WebConfigurationManager.AppSettings["KartkatalogUrl"] + "api/metadataupdated";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = WebRequestMethods.Http.Post;
                    request.ContentType = "application/x-www-form-urlencoded";
                    using (var writer = new StreamWriter(request.GetRequestStream()))
                    {
                        writer.Write("uuid=" + uuid);
                        writer.Write("&action=post");
                    }
                    NetworkCredential networkCredential = new NetworkCredential(username, password);
                    CredentialCache myCredentialCache = new CredentialCache { { new Uri(url), "Basic", networkCredential } };
                    request.PreAuthenticate = true;
                    request.Credentials = myCredentialCache;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                }
            }
        }

        private void RemoveCache(MetadataViewModel metadata)
        {            
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(System.Web.Configuration.WebConfigurationManager.AppSettings["KartkatalogUrl"] + "thumbnail/removecache/" + metadata.Uuid);

            HttpWebResponse response = (HttpWebResponse)
            request.GetResponse();

        }

        private List<SimpleDistribution> SetEnglishTranslationForUnitsOfDistributions(List<SimpleDistribution> unitsOfDistributions)
        {
            //Get translation
            Dictionary<string, string> CodeValues = new Dictionary<string, string>();
            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/kodelister/9A46038D-16EE-4562-96D2-8F6304AAB119";
            System.Net.WebClient client = new System.Net.WebClient();
            client.Headers.Remove("Accept-Language");
            client.Headers.Add("Accept-Language", Translations.Culture.EnglishCode);
            client.Encoding = System.Text.Encoding.UTF8;
            var data = client.DownloadString(url);
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var codeList = response["containeditems"];

            foreach (var code in codeList)
            {
                JToken codevalueToken = code["codevalue"];
                string codevalue = codevalueToken?.ToString();

                if (string.IsNullOrWhiteSpace(codevalue))
                    codevalue = code["label"].ToString();

                if (!CodeValues.ContainsKey(codevalue))
                {
                    CodeValues.Add(codevalue, code["label"].ToString());
                }
            }

            for (int i = 0; i < unitsOfDistributions.Count; i++)
            {
                string unitsOfDistributionEnglish = "";

                if (!string.IsNullOrEmpty(unitsOfDistributions[i].UnitsOfDistribution))
                { 
                var units = unitsOfDistributions[i].UnitsOfDistribution.Split(',');
                    for(int u=0; u< units.Length; u++)
                    {
                        if (CodeValues.ContainsKey(units[u].Trim()))
                            unitsOfDistributionEnglish = unitsOfDistributionEnglish + CodeValues[units[u].Trim()];
                        if (u != units.Length - 1)
                            unitsOfDistributionEnglish = unitsOfDistributionEnglish + ", ";
                    }
                }

                unitsOfDistributions[i].EnglishUnitsOfDistribution = unitsOfDistributionEnglish;
            }

            return unitsOfDistributions;
        }

        private void UpdateMetadataFromModel(MetadataViewModel model, SimpleMetadata metadata)
        {
            metadata.Title = model.Title;
            metadata.Abstract = model.Abstract;

            if (!string.IsNullOrEmpty(model.ParentIdentifier))
                metadata.ParentIdentifier = model.ParentIdentifier;

            metadata.Purpose = !string.IsNullOrWhiteSpace(model.Purpose) ? model.Purpose : " ";

            if (!string.IsNullOrWhiteSpace(model.TopicCategory))
                metadata.TopicCategory = model.TopicCategory;

            metadata.SupplementalDescription = model.SupplementalDescription;

            metadata.SpecificUsage = !string.IsNullOrWhiteSpace(model.SpecificUsage) ? model.SpecificUsage : " ";

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

            // documents
            metadata.ProductSpecificationUrl = model.ProductSpecificationUrl;

            metadata.ApplicationSchema = model.ApplicationSchema;

            if (metadata.IsDataset()) { 
                metadata.ProductSpecificationOther = new SimpleOnlineResource
                {
                    Name = model.ProductSpecificationOther.Name,
                    URL = model.ProductSpecificationOther.URL
                };
            }


            metadata.ProductSheetUrl = model.ProductSheetUrl;
                metadata.ProductPageUrl = model.ProductPageUrl;
                metadata.LegendDescriptionUrl = model.LegendDescriptionUrl;
                metadata.CoverageUrl = model.CoverageUrl;
            metadata.HelpUrl = model.HelpUrl;

            metadata.Thumbnails = Thumbnail.ToSimpleThumbnailList(model.Thumbnails);

            // distribution
            metadata.SpatialRepresentation = model.SpatialRepresentation;

            var refsys = model.GetReferenceSystems();
            if (refsys != null)
                metadata.ReferenceSystems = refsys;

            var distribution = model.GetDistributionsFormats();
            distribution = SetEnglishTranslationForUnitsOfDistributions(distribution);


            if (model.IsDataset() || model.IsDatasetSeries())
            {
                metadata.DistributionsFormats = distribution;

                if (metadata.DistributionsFormats != null && metadata.DistributionsFormats.Count > 0)
                {
                    metadata.DistributionDetails = new SimpleDistributionDetails
                    {
                        URL = metadata.DistributionsFormats[0].URL,
                        Protocol = metadata.DistributionsFormats[0].Protocol,
                        Name = metadata.DistributionsFormats[0].Name,
                        UnitsOfDistribution = metadata.DistributionsFormats[0].UnitsOfDistribution,
                        EnglishUnitsOfDistribution = metadata.DistributionsFormats[0].EnglishUnitsOfDistribution
                    };
                }
            }
            else
            {
                List<SimpleDistributionFormat> formats = new List<SimpleDistributionFormat>();
                foreach(var format in distribution)
                {
                    formats.Add(new SimpleDistributionFormat { Name = format.FormatName, Version = format.FormatVersion });
                }
                metadata.DistributionFormats = formats;

                if (distribution != null && distribution.Count > 0)
                {
                    metadata.DistributionDetails = new SimpleDistributionDetails
                    {
                        URL = distribution[0].URL,
                        Protocol = distribution[0].Protocol,
                        Name = distribution[0].Name,
                        UnitsOfDistribution = distribution[0].UnitsOfDistribution,
                        EnglishUnitsOfDistribution = metadata.DistributionsFormats[0].EnglishUnitsOfDistribution
                    };
                }
            }


            // quality
            List<SimpleQualitySpecification> qualityList = new List<SimpleQualitySpecification>();
            if (!string.IsNullOrWhiteSpace(model.QualitySpecificationTitleInspire))
            {
                qualityList.Add(new SimpleQualitySpecification
                {
                    Title = model.QualitySpecificationTitleInspire,
                    Date = string.Format("{0:yyyy-MM-dd}", model.QualitySpecificationDateInspire),
                    DateType = model.QualitySpecificationDateTypeInspire,
                    Explanation = model.QualitySpecificationExplanationInspire,
                    EnglishExplanation = model.EnglishQualitySpecificationExplanationInspire,
                    Result = model.QualitySpecificationResultInspire,
                    Responsible = "inspire"
                });
            }
            if (!string.IsNullOrWhiteSpace(model.QualitySpecificationTitleSosi))
            {
                qualityList.Add(new SimpleQualitySpecification
                {
                    Title = model.QualitySpecificationTitleSosi,
                    Date = string.Format("{0:yyyy-MM-dd}", model.QualitySpecificationDateSosi),
                    DateType = model.QualitySpecificationDateTypeSosi,
                    Explanation = model.QualitySpecificationExplanationSosi,
                    EnglishExplanation = model.EnglishQualitySpecificationExplanationSosi,
                    Result = model.QualitySpecificationResultSosi,
                    Responsible = "sosi"
                });
            }

            if (!string.IsNullOrWhiteSpace(model.QualitySpecificationTitleInspire))
            {
                if (model.QualitySpecificationResultSosiConformApplicationSchema)
                {
                    qualityList.Add(new SimpleQualitySpecification
                    {
                        Title = "Sosi applikasjonsskjema",
                        Date = string.Format("{0:yyyy-MM-dd}", model.QualitySpecificationDateSosi),
                        DateType = model.QualitySpecificationDateTypeSosi,
                        Explanation = "SOSI-filer er i henhold til applikasjonsskjema",
                        EnglishExplanation = "SOSI files are according to application form",
                        Result = true,
                        Responsible = "uml-sosi"
                    });
                }
                else
                {
                    qualityList.Add(new SimpleQualitySpecification
                    {
                        Title = "Sosi applikasjonsskjema",
                        Date = string.Format("{0:yyyy-MM-dd}", model.QualitySpecificationDateSosi),
                        DateType = model.QualitySpecificationDateTypeSosi,
                        Explanation = "SOSI-filer avviker fra applikasjonsskjema",
                        EnglishExplanation = "SOSI files are not according to application form",
                        Result = false,
                        Responsible = "uml-sosi"
                    });
                }
                if (model.QualitySpecificationResultSosiConformGmlApplicationSchema)
                {
                    qualityList.Add(new SimpleQualitySpecification
                    {
                        Title = "Sosi applikasjonsskjema",
                        Date = string.Format("{0:yyyy-MM-dd}", model.QualitySpecificationDateSosi),
                        DateType = model.QualitySpecificationDateTypeSosi,
                        Explanation = "GML-filer er i henhold til applikasjonsskjema",
                        EnglishExplanation = "GML files are according to application form",
                        Result = true,
                        Responsible = "uml-gml"
                    });
                }
                else
                {
                    qualityList.Add(new SimpleQualitySpecification
                    {
                        Title = "Sosi applikasjonsskjema",
                        Date = string.Format("{0:yyyy-MM-dd}", model.QualitySpecificationDateSosi),
                        DateType = model.QualitySpecificationDateTypeSosi,
                        Explanation = "GML-filer avviker fra applikasjonsskjema",
                        EnglishExplanation = "GML files are not according to application form",
                        Result = false,
                        Responsible = "uml-gml"
                    });
                }
            }
            if (!string.IsNullOrWhiteSpace(model.QualitySpecificationTitle))
            {
                qualityList.Add(new SimpleQualitySpecification
                {
                    Title = model.QualitySpecificationTitle,
                    Date = string.Format("{0:yyyy-MM-dd}", model.QualitySpecificationDate),
                    DateType = model.QualitySpecificationDateType,
                    Explanation = model.QualitySpecificationExplanation,
                    EnglishExplanation = model.EnglishQualitySpecificationExplanation,
                    Result = model.QualitySpecificationResult,
                    Responsible = "other"
                });
            }

            metadata.QualitySpecifications = qualityList;

            metadata.ProcessHistory = !string.IsNullOrWhiteSpace(model.ProcessHistory) ? model.ProcessHistory : " ";

            if (!string.IsNullOrWhiteSpace(model.MaintenanceFrequency))
                metadata.MaintenanceFrequency = model.MaintenanceFrequency;

            if (!model.IsService())
                metadata.ResolutionScale = !string.IsNullOrWhiteSpace(model.ResolutionScale) ? model.ResolutionScale : " ";

            if (!string.IsNullOrWhiteSpace(model.Status))
                metadata.Status = model.Status;

            metadata.DateCreated = model.DateCreated;
            metadata.DatePublished = model.DatePublished;
            metadata.DateUpdated = model.DateUpdated;

            DateTime? DateMetadataValidFrom = model.DateMetadataValidFrom;
            DateTime? DateMetadataValidTo = model.DateMetadataValidTo;

                metadata.ValidTimePeriod = new SimpleValidTimePeriod()
                {
                    ValidFrom = DateMetadataValidFrom != null ? String.Format("{0:yyyy-MM-dd}", DateMetadataValidFrom) : "",
                    ValidTo = DateMetadataValidTo != null ? String.Format("{0:yyyy-MM-dd}", DateMetadataValidTo) : ""
                };


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

            var accessConstraintsSelected = model.AccessConstraints;
            string otherConstraintsAccess = model.OtherConstraintsAccess; 

            if (!string.IsNullOrEmpty(accessConstraintsSelected))
            {
                if (accessConstraintsSelected.ToLower() == "no restrictions" || accessConstraintsSelected.ToLower() == "norway digital restricted")
                {
                    otherConstraintsAccess = accessConstraintsSelected;
                    accessConstraintsSelected = "otherRestrictions";

                }
            }

            metadata.Constraints = new SimpleConstraints
            {
                AccessConstraints = !string.IsNullOrWhiteSpace(accessConstraintsSelected) ? accessConstraintsSelected : "",
                OtherConstraints = !string.IsNullOrWhiteSpace(model.OtherConstraints) ? model.OtherConstraints : "",
                EnglishOtherConstraints = !string.IsNullOrWhiteSpace(model.EnglishOtherConstraints) ? model.EnglishOtherConstraints : "",
                OtherConstraintsLink = !string.IsNullOrWhiteSpace(model.OtherConstraintsLink) ? model.OtherConstraintsLink : null,
                OtherConstraintsLinkText = !string.IsNullOrWhiteSpace(model.OtherConstraintsLinkText) ? model.OtherConstraintsLinkText : null,
                SecurityConstraints = !string.IsNullOrWhiteSpace(model.SecurityConstraints) ? model.SecurityConstraints : "",
                SecurityConstraintsNote = !string.IsNullOrWhiteSpace(model.SecurityConstraintsNote) ? model.SecurityConstraintsNote : "",
                UseConstraints = !string.IsNullOrWhiteSpace(model.UseConstraints) ? model.UseConstraints : "",
                UseLimitations = !string.IsNullOrWhiteSpace(model.UseLimitations) ? model.UseLimitations : "",
                EnglishUseLimitations = !string.IsNullOrWhiteSpace(model.EnglishUseLimitations) ? model.EnglishUseLimitations : "",
                OtherConstraintsAccess = !string.IsNullOrWhiteSpace(otherConstraintsAccess) ? otherConstraintsAccess : "",
            };

            if(model.IsService() && model.DistributionsFormats != null && model.DistributionsFormats.Count > 0)
            { 
                model.KeywordsServiceType = AddKeywordForService(model);
                metadata.ServiceType = GetServiceType(model.DistributionsFormats[0].Protocol);
            }
            metadata.Keywords = model.GetAllKeywords();

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

            if (!string.IsNullOrWhiteSpace(model.EnglishPurpose))
            {
                metadata.EnglishPurpose = model.EnglishPurpose;
                hasEnglishFields = true;
            }

            if (!string.IsNullOrWhiteSpace(model.EnglishSupplementalDescription))
            {
                metadata.EnglishSupplementalDescription = model.EnglishSupplementalDescription;
                hasEnglishFields = true;
            }

            if (!string.IsNullOrWhiteSpace(model.EnglishSpecificUsage))
            {
                metadata.EnglishSpecificUsage = model.EnglishSpecificUsage;
                hasEnglishFields = true;
            }

            if (!string.IsNullOrWhiteSpace(model.EnglishProcessHistory))
            {
                metadata.EnglishProcessHistory = model.EnglishProcessHistory;
                hasEnglishFields = true;
            }

            if (hasEnglishFields)
                metadata.SetLocale(SimpleMetadata.LOCALE_ENG);

            if (model.OperatesOn != null)
                metadata.OperatesOn = model.OperatesOn;

            if (model.CrossReference != null)
                metadata.CrossReference = model.CrossReference;

            if (!string.IsNullOrWhiteSpace(model.ResourceReferenceCode) || !string.IsNullOrWhiteSpace(model.ResourceReferenceCodespace))
            {
                metadata.ResourceReference = new SimpleResourceReference
                {
                 Code = model.ResourceReferenceCode != null ? model.ResourceReferenceCode : null, 
                 Codespace = model.ResourceReferenceCodespace != null ? model.ResourceReferenceCodespace : null 
                };
            }

            if (model.IsService())
                metadata.AccessProperties = new SimpleAccessProperties { OrderingInstructions = model.OrderingInstructions }  ;

            SetDefaultValuesOnMetadata(metadata);
        }

        private string GetServiceType(string distributionProtocol)
        {
            string serviceType = "other";

            switch (distributionProtocol)
            {
                case "OGC:WMS":
                    serviceType = "view";
                    break;
                case "OGC:WMTS":
                    serviceType = "view";
                    break;
                case "OGC:WFS":
                    serviceType = "download";
                    break;
                case "OGC:WCS":
                    serviceType = "download";
                    break;
                case "OGC:CSW":
                    serviceType = "discovery";
                    break;
                case "OGC:WPS":
                    serviceType = "other";
                    break;
                case "OGC:SOS":
                    serviceType = "other";
                    break;
                case "REST-API":
                    serviceType = "other";
                    break;
            }

            return serviceType;
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

                if ( !Decimal.TryParse(WestBoundLongitude, out number)
                    || !Decimal.TryParse(EastBoundLongitude, out number)
                    || !Decimal.TryParse(SouthBoundLatitude, out number)
                    || !Decimal.TryParse(NorthBoundLatitude, out number)
                    )
                {
                    WestBoundLongitude = parentWestBoundLongitude;
                    EastBoundLongitude = parentEastBoundLongitude;
                    SouthBoundLatitude = parentSouthBoundLatitude;
                    NorthBoundLatitude = parentNorthBoundLatitude;

                         if ( !Decimal.TryParse(WestBoundLongitude, out number)
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

        public Stream SaveMetadataAsXml(MetadataViewModel model)
        {
            var simpleMetadata = new SimpleMetadata(_geoNorge.GetRecordByUuid(model.Uuid));
            UpdateMetadataFromModel(model, simpleMetadata);
            return SerializeUtil.SerializeToStream(simpleMetadata.GetMetadata());
        }

        string GetServiceKeyword(string distributionProtocol)
        {
            string keyword = null;

            switch (distributionProtocol)
            {
                case "OGC:WMS":
                    keyword = "infoMapAccessService";
                    break;
                case "OGC:WMTS":
                    keyword = "infoMapAccessService";
                    break;
                case "OGC:WFS":
                    keyword = "infoFeatureAccessService";
                    break;
                case "OGC:WCS":
                    keyword = "infoCoverageAccessService";
                    break;
                case "OGC:CSW":
                    keyword = "infoCatalogueService";
                    break;
                case "OGC:WPS":
                    keyword = "spatialProcessingService";
                    break;
                case "OGC:SOS":
                    keyword = "";
                    break;
                case "REST-API":
                    keyword = "infoFeatureAccessService";
                    break;
            }

            return keyword;
        }

        private List<string> AddKeywordForService(MetadataViewModel model)
        {
            string serviceKeyword = GetServiceKeyword(model.DistributionsFormats[0].Protocol);
            if (!string.IsNullOrEmpty(serviceKeyword) && !model.KeywordsServiceType.Contains(serviceKeyword)) {
                foreach (var serviceDistribution in model.ServiceDistributionKeywords)
                {
                    model.KeywordsOther.Remove(serviceDistribution.Key); // Remove from old storage
                    model.KeywordsServiceType.Remove(serviceDistribution.Key);
                }

                model.KeywordsServiceType.Add(serviceKeyword);
            }

            return model.KeywordsServiceType;
        }
    }
}