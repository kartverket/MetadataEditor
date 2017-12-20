using System.Runtime.CompilerServices;
using ExpressiveAnnotations.Attributes;
using GeoNorgeAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Resources;
using System.Linq;

namespace Kartverket.MetadataEditor.Models
{
    public class MetadataViewModel
    {

        public MetadataViewModel()
        {
            KeywordsTheme = new List<string>();
            KeywordsPlace = new List<string>();
            KeywordsInspire = new List<string>();
            KeywordsServiceTaxonomy = new List<string>();
            KeywordsNationalInitiative = new List<string>();
            KeywordsNationalTheme = new List<string>();
            KeywordsConcept = new List<string>();
            KeywordsServiceType = new List<string>();
            KeywordsOther = new List<string>();
            KeywordsEnglish = new Dictionary<string, string>();
            Thumbnails = new List<Thumbnail>();
            OperatesOn = new List<string>();
            CrossReference = new List<string>();
        }
        
        public string Uuid { get; set; }
        public string HierarchyLevel { get; set; }
        public string ParentIdentifier { get; set; }
        public string MetadataStandard { get; set; }

        [Required(ErrorMessage=null)]
        [Display(Name = "Metadata_Title", ResourceType = typeof(UI))]
        public string Title { get; set; }

        //[Required(ErrorMessage = null)]
        [Display(Name = "Metadata_Purpose", ResourceType = typeof(UI))]
        public string Purpose { get; set; }

        [Required(ErrorMessage = "Sammendrag er påkrevd")]
        [Display(Name = "Metadata_Abstract", ResourceType = typeof(UI))]
        public string Abstract { get; set; }

        public Contact ContactMetadata { get; set; }
        public Contact ContactPublisher { get; set; }
        public Contact ContactOwner { get; set; }

        public List<String> KeywordsTheme { get; set; }
        public List<String> KeywordsPlace { get; set; }
        public List<String> KeywordsInspire { get; set; }
        public List<String> KeywordsServiceTaxonomy { get; set; }
        public List<String> KeywordsNationalInitiative { get; set; }
        public List<String> KeywordsNationalTheme { get; set; }
        public List<String> KeywordsConcept { get; set; }
        public List<String> KeywordsServiceType { get; set; }
        public List<String> KeywordsOther { get; set; }
        public Dictionary<string, string> KeywordsEnglish { get; set; }

        public string LegendDescriptionUrl { get; set; }
        public string ProductPageUrl { get; set; }
        public string ProductSheetUrl { get; set; }
        public string ProductSpecificationUrl { get; set; }
        public string ApplicationSchema { get; set; }
        public SimpleOnlineResource ProductSpecificationOther { get; set; }
        public string CoverageUrl { get; set; }
        public string HelpUrl { get; set; }

        public List<Thumbnail> Thumbnails { get; set; }

        public string Status { get; set; }
        public string OrderingInstructions { get; set; }    

        [Required (ErrorMessage="Geografisk utstrekning nord er påkrevd")]
        [RegularExpression(@"-?([0-9]+)(\.[0-9]+)?", ErrorMessageResourceName = "Metadata_BoundingBox_East_Invalid", ErrorMessageResourceType = typeof(UI), ErrorMessage=null)]
        public string BoundingBoxEast { get; set; }

        [Required(ErrorMessage = "Geografisk utstrekning vest er påkrevd")]
        [RegularExpression(@"-?([0-9]+)(\.[0-9]+)?", ErrorMessageResourceName = "Metadata_BoundingBox_West_Invalid", ErrorMessageResourceType = typeof(UI), ErrorMessage = null)]
        public string BoundingBoxWest { get; set; }

        [Required(ErrorMessage = "Geografisk utstrekning nord er påkrevd")]
        [RegularExpression(@"-?([0-9]+)(\.[0-9]+)?", ErrorMessageResourceName = "Metadata_BoundingBox_North_Invalid", ErrorMessageResourceType = typeof(UI), ErrorMessage = null)]
        public string BoundingBoxNorth { get; set; }

        [Required(ErrorMessage = "Geografisk utstrekning sør er påkrevd")]
        [RegularExpression(@"-?([0-9]+)(\.[0-9]+)?", ErrorMessageResourceName = "Metadata_BoundingBox_South_Invalid", ErrorMessageResourceType = typeof(UI), ErrorMessage = null)]
        public string BoundingBoxSouth { get; set; }

        /* dataset only */
        public string SupplementalDescription { get; set; }
        //[Required (ErrorMessage="Bruksområde er påkrevd")]
        public string SpecificUsage { get; set; }  // bruksområde
        public string ResourceIdentifierName { get; set; }  // teknisk navn
        public string TopicCategory { get; set; }
        public string SpatialRepresentation { get; set; }

        public string DistributionFormatName { get; set; }
        public string DistributionFormatVersion { get; set; }
        //[AssertThat("IsValidDistributionFormat()", ErrorMessage = "Distribusjonsformat er påkrevd")]
        public List<SimpleDistributionFormat> DistributionFormats { get; set; }
        [AssertThat("IsValidDistributionsFormat()", ErrorMessage = "Distribusjonsformat er påkrevd")]
        public List<SimpleDistribution> DistributionsFormats { get; set; }
        public Dictionary<DistributionGroup, Distribution> FormatDistributions{ get; set; }
        public string DistributionUrl { get; set; }
        //[Required(ErrorMessage = "Distribusjonstype er påkrevd")]
        public string DistributionProtocol { get; set; }
        public string DistributionName { get; set; }
        public string UnitsOfDistribution { get; set; }

        public string ReferenceSystemCoordinateSystem { get; set; }
        public string ReferenceSystemNamespace { get; set; }
        public List<SimpleReferenceSystem> ReferenceSystems { get; set; }

        public string ResourceReferenceCode { get; set; }
        public string ResourceReferenceCodespace { get; set; }

        
        // quality
        public DateTime? QualitySpecificationDate { get; set; }
        public DateTime? QualitySpecificationDateInspire { get; set; }
        public DateTime? QualitySpecificationDateSosi { get; set; }
        public string QualitySpecificationDateType { get; set; }
        public string QualitySpecificationDateTypeInspire { get; set; }
        public string QualitySpecificationDateTypeSosi { get; set; }
        [Display(Name = "Metadata_QualitySpecificationExplanation", ResourceType = typeof(UI))]
        [RequiredIf("!IsValidQualitySpesification()", ErrorMessage = "Konformitet annen standard mangler")]
        public string QualitySpecificationExplanation { get; set; }
        public string EnglishQualitySpecificationExplanation { get; set; }
        [RequiredIf("!IsValidQualitySpesificationInspire()", ErrorMessage = "Konformitet inspire mangler")]
        public string QualitySpecificationExplanationInspire { get; set; }
        public string EnglishQualitySpecificationExplanationInspire { get; set; }
        [RequiredIf("!IsValidQualitySpesificationSosi()", ErrorMessage = "Konformitet sosi mangler")]
        public string QualitySpecificationExplanationSosi { get; set; }
        public string EnglishQualitySpecificationExplanationSosi { get; set; }
        [Required(ErrorMessage = "Datakonformitet er påkrevd")]
        public bool QualitySpecificationResult { get; set; }
        [Required(ErrorMessage = "Datakonformitet inspire er påkrevd")]
        public bool QualitySpecificationResultInspire { get; set; }
        [Required(ErrorMessage = "Datakonformitet sosi er påkrevd")]
        public bool QualitySpecificationResultSosi { get; set; }
        public string QualitySpecificationTitle { get; set; }
        public string QualitySpecificationTitleInspire { get; set; }
        public string QualitySpecificationTitleSosi { get; set; }
        public bool QualitySpecificationResultSosiConformApplicationSchema { get; set; } = false;
        public bool QualitySpecificationResultSosiConformGmlApplicationSchema { get; set; } = false;
        //[Required(ErrorMessage = "Prosesshistorie er påkrevd")]
        public string ProcessHistory { get; set; }
        [Required (ErrorMessage="Oppdateringshyppighet er påkrevd")]
        public string MaintenanceFrequency { get; set; }
        //[RequiredIf("!IsService()", ErrorMessage = "Målestokktall er påkrevd")]
        public string ResolutionScale { get; set; }

        // constraints
        public string UseLimitations { get; set; }
        public string EnglishUseLimitations { get; set; }
        public string AccessConstraints { get; set; }
        public string UseConstraints { get; set; }
        public string OtherConstraints { get; set; }
        public string EnglishOtherConstraints { get; set; }
        public string OtherConstraintsLink { get; set; }
        public string OtherConstraintsLinkText { get; set; }
        public string OtherConstraintsAccess { get; set; }
        public string SecurityConstraints { get; set; }
        public string SecurityConstraintsNote { get; set; }

        [Display(Name = "Metadata_DateCreated", ResourceType = typeof(UI))]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Metadata_DatePublished", ResourceType = typeof(UI))]
        [DisplayFormat(NullDisplayText = "", ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public DateTime? DatePublished { get; set; }

        [Display(Name = "Metadata_DateUpdated", ResourceType = typeof(UI))]
        [DisplayFormat(NullDisplayText = "", ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public DateTime? DateUpdated { get; set; }

        public DateTime? DateMetadataUpdated { get; set; }

        public DateTime? DateMetadataValidFrom { get; set; }
        public DateTime? DateMetadataValidTo { get; set; }

        public string EnglishTitle { get; set; }
        public string EnglishAbstract { get; set; }
        public string EnglishPurpose { get; set; }
        public string EnglishSupplementalDescription { get; set; }
        public string EnglishSpecificUsage { get; set; }
        public string EnglishProcessHistory { get; set; }

        public string EnglishContactMetadataOrganization { get; set; }
        public string EnglishContactPublisherOrganization { get; set; }
        public string EnglishContactOwnerOrganization { get; set; }

        public List<string> OperatesOn { get; set; }

        public List<string> CrossReference { get; set; }

        public string Published { get; set; }

        public bool ValidateAllRequirements { get; set; }


        public bool IsValidQualitySpesification() 
        {
            if (!ValidateAllRequirements)
                return true;

            if (IsSoftware())
                return true;

            if (ProductSpecificationOther == null)
                return true;

            if (string.IsNullOrEmpty(ProductSpecificationOther.Name) && string.IsNullOrEmpty(ProductSpecificationOther.URL))
                return true;

            if (string.IsNullOrEmpty(QualitySpecificationTitle) && string.IsNullOrEmpty(QualitySpecificationExplanation))
                return false;
            else
                return true;


        }

        public bool IsValidQualitySpesificationInspire()
        {
            if (!ValidateAllRequirements)
                return true;

            if (IsSoftware())
                return true;

            bool IsInspire = false;

            if (KeywordsNationalInitiative != null)
            {
                if (!KeywordsNationalInitiative.Contains("Inspire"))
                    return true;
                else
                    IsInspire = true;
            }

            if (IsInspire && (string.IsNullOrEmpty(QualitySpecificationTitleInspire) || string.IsNullOrEmpty(QualitySpecificationExplanationInspire)))
                return false;
            else
                return true;

        }

        public bool IsValidQualitySpesificationSosi()
        {
            if (!ValidateAllRequirements)
                return true;

            if (IsSoftware())
                return true;

            bool IsSosi = false;

            if (KeywordsNationalInitiative != null)
            {
                if (KeywordsNationalInitiative.Contains("Det offentlige kartgrunnlaget") || KeywordsNationalInitiative.Contains("geodataloven")
                    || KeywordsNationalInitiative.Contains("Norge digitalt") || KeywordsNationalInitiative.Contains("arealplanerPBL") )
                    IsSosi = true;
            }

            if (!string.IsNullOrEmpty(ApplicationSchema) && string.IsNullOrEmpty(ProductSpecificationUrl) && IsSosi)
                return true;

            if (IsSosi && (string.IsNullOrEmpty(QualitySpecificationTitleSosi) || string.IsNullOrEmpty(QualitySpecificationExplanationSosi)))
                return false;
            else
                return true;

        }

        internal void FixThumbnailUrls()
        {
            foreach (var thumbnail in Thumbnails)
            {
                if (!string.IsNullOrEmpty(thumbnail.URL) && !thumbnail.URL.StartsWith("http"))
                {
                    thumbnail.URL = "https://www.geonorge.no/geonetwork/srv/nor/resources.get?uuid=" + Uuid + "&access=public&fname=" + thumbnail.URL;
                }
            }
        }

        internal List<SimpleKeyword> CreateKeywords(List<string> inputList, string prefix, string type = null, string thesaurus = null)
        {
            List<SimpleKeyword> output = new List<SimpleKeyword>();
            
            if (inputList != null)
            {
                inputList = inputList.Distinct().ToList();

                foreach (var keyword in inputList)
                {
                    string keywordString = keyword;
                    string keywordLink = null;
                    if (keyword.Contains("|")) { 
                        keywordString = keyword.Split('|')[0];
                        keywordLink = keyword.Split('|')[1];
                    }
                    string englishTranslation = null;
                    string keyForEnglishTranslation = prefix + "_" + keywordString;
                    if (KeywordsEnglish != null && KeywordsEnglish.ContainsKey(keyForEnglishTranslation))
                    {
                        englishTranslation = KeywordsEnglish[keyForEnglishTranslation];
                    }

                    output.Add(new SimpleKeyword
                    {
                        Keyword = keywordString,
                        KeywordLink = keywordLink,
                        Thesaurus = thesaurus,
                        Type = type,
                        EnglishKeyword = englishTranslation,
                    });
                }
            }
            return output;
        }

        internal List<SimpleKeyword> GetAllKeywords()
        {
            List<SimpleKeyword> allKeywords = new List<SimpleKeyword>();
            allKeywords.AddRange(CreateKeywords(KeywordsInspire, "Inspire", null, SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1));
            allKeywords.AddRange(CreateKeywords(KeywordsNationalInitiative, "NationalInitiative", null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE));
            allKeywords.AddRange(CreateKeywords(KeywordsNationalTheme, "NationalTheme", null, SimpleKeyword.THESAURUS_NATIONAL_THEME));
            allKeywords.AddRange(CreateKeywords(KeywordsServiceTaxonomy, "Service", null, SimpleKeyword.THESAURUS_SERVICES_TAXONOMY));
            allKeywords.AddRange(CreateKeywords(KeywordsPlace, "Place", SimpleKeyword.TYPE_PLACE, null));
            allKeywords.AddRange(CreateKeywords(KeywordsTheme, "Theme", SimpleKeyword.TYPE_THEME, null));
            allKeywords.AddRange(CreateKeywords(KeywordsConcept, "Concept", null , SimpleKeyword.THESAURUS_CONCEPT));
            allKeywords.AddRange(CreateKeywords(KeywordsServiceType, "ServiceType", null, SimpleKeyword.THESAURUS_SERVICE_TYPE));
            allKeywords.AddRange(CreateKeywords(KeywordsOther, "Other", null, null));
            return allKeywords;
        }

        internal List<SimpleDistribution> GetDistributionsFormats()
        {
            List<SimpleDistribution> distributionsFormats = new List<SimpleDistribution>();

            if(DistributionsFormats != null)
            { 
                for (int d = 0; d < DistributionsFormats.Count; d++)
                {
                    SimpleDistribution distributionFormat = new SimpleDistribution();
                    distributionFormat.Organization = DistributionsFormats[d].Organization;
                    distributionFormat.FormatName = DistributionsFormats[d].FormatName;
                    distributionFormat.FormatVersion = DistributionsFormats[d].FormatVersion;
                    distributionFormat.Protocol = DistributionsFormats[d].Protocol;
                    distributionFormat.URL = DistributionsFormats[d].URL;
                    distributionFormat.Name = DistributionsFormats[d].Name;
                    distributionFormat.UnitsOfDistribution = DistributionsFormats[d].UnitsOfDistribution;
                    distributionsFormats.Add(distributionFormat);
                }
            }
            return distributionsFormats;

        }

        internal List<SimpleReferenceSystem> GetReferenceSystems()
        {
            if (ReferenceSystems == null)
                return null;

            List<SimpleReferenceSystem> referenceSystems = new List<SimpleReferenceSystem>();

            for (int r = 0; r < ReferenceSystems.Count; r++)
            {
                SimpleReferenceSystem referenceSystem = new SimpleReferenceSystem();
                referenceSystem.CoordinateSystem = ReferenceSystems[r].CoordinateSystem;
                referenceSystem.Namespace = ReferenceSystems[r].Namespace;
                referenceSystems.Add(referenceSystem);
            }

            return referenceSystems;

        }


        internal bool HasAccess(string organization)
        {
            bool hasAccess = false;

            if (ContactMetadata != null && ContactMetadata.Organization != null && ContactMetadata.Organization.ToLower().Equals(organization.ToLower()))
            {
                hasAccess = true;
            }

            return hasAccess;
        }


        public bool IsService()
        {
            return HierarchyLevel == "service";
        }

        public bool IsDataset()
        {
            return HierarchyLevel == "dataset";
        }

        public bool IsSoftware()
        {
            return HierarchyLevel == "software";
        }

        public bool IsDatasetSeries()
        {
            return HierarchyLevel == "series";
        }

        public bool IsDatasetOrSeriesOrSoftware()
        {
            return IsDataset() || IsDatasetSeries() || IsSoftware();
        }

        public bool IsValidDistributionsFormat() {

            bool valid = false;

            if (IsSoftware())
                return true;

            if ((IsDataset() || IsDatasetSeries()) && DistributionsFormats != null && DistributionsFormats.Count > 0 &&
               !string.IsNullOrWhiteSpace(DistributionsFormats[0].FormatName) && !string.IsNullOrWhiteSpace(DistributionsFormats[0].Organization))
                return true;

            if ((!IsDataset() && !IsDatasetSeries()) &&  DistributionsFormats != null && DistributionsFormats.Count > 0 &&
               !string.IsNullOrWhiteSpace(DistributionsFormats[0].FormatName))
                return true;

            return valid;
        }

        public bool IsValidDistributionFormat()
        {
            return (!string.IsNullOrWhiteSpace(DistributionFormats[0].Name) || IsSoftware());
        }

        public string GetInnholdstypeCSS()
        {
            string t = "label-default";
            if (HierarchyLevel == "dataset") t = "label-datasett";
            else if (HierarchyLevel == "service" && (DistributionProtocol != null && !string.IsNullOrWhiteSpace(DistributionName))) t = "label-tjenestelag";
            else if (HierarchyLevel == "service") t = "label-tjeneste";
            else if (HierarchyLevel == "software") t = "label-applikasjon";
            else if (HierarchyLevel == "series") t = "label-datasettserie";
            else if (HierarchyLevel == "dimensionGroup") t = "label-datasett";
            return t;
        }

        public string GetInnholdstype()
        {
            string t = HierarchyLevel;
            if (HierarchyLevel == "dataset") t = "Datasett";
            else if (HierarchyLevel == "service" && (DistributionProtocol != null && !string.IsNullOrWhiteSpace(DistributionName))) t = "Tjenestelag";
            else if (HierarchyLevel == "service") t = "Tjeneste";
            else if (HierarchyLevel == "software") t = "Applikasjon";
            else if (HierarchyLevel == "series") t = "Datasettserie";
            else if (HierarchyLevel == "dimensionGroup") t = "Datapakke";
            return t;
        }

        public Dictionary<string, string> ServiceDistributionKeywords = 
            new Dictionary<string, string>
            {
                { "infoMapAccessService"  , "infoMapAccessService" },
                { "infoFeatureAccessService"  , "infoFeatureAccessService" },
                { "infoCoverageAccessService"  , "infoCoverageAccessService" },
                { "infoCatalogueService"  , "infoCatalogueService" },
                { "spatialProcessingService"  , "spatialProcessingService" }
            };

    }

    public class Contact
    {
        [Display(Name = "Contact_Name", ResourceType = typeof(UI))]
        public string Name { get; set; }


        [Required(ErrorMessageResourceName = "Metadata_Contact_Organization_Required", ErrorMessageResourceType = typeof(UI), ErrorMessage=null)]
        [Display(Name = "Contact_Organization", ResourceType = typeof(UI))]
        public string Organization { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = null)]
        public string Email { get; set; }

        public string Role { get; set; }

        public Contact() { }
        public Contact(SimpleContact incoming, string defaultRole) 
        {
            if (incoming != null)
            {
                Name = incoming.Name;
                Organization = incoming.Organization;
                Email = incoming.Email;
                Role = incoming.Role;
            }
            else
            {
                Role = defaultRole;
            }
        }

        internal SimpleContact ToSimpleContact()
        {
            return new SimpleContact
            {
                Name = Name,
                Organization = Organization,
                Email = Email,
                Role = Role
            };
        }
    }


    public class Keyword
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public string Thesaurus { get; set; }

        public Keyword() { }

        public Keyword(SimpleKeyword simple)
        {
            Value = simple.Keyword;
            Thesaurus = simple.Thesaurus;
            Type = simple.Type;
        }

        internal static Dictionary<string, List<Keyword>> CreateDictionary(List<SimpleKeyword> incoming)
        {
            Dictionary<string, List<Keyword>> output = new Dictionary<string, List<Keyword>>();

            if (incoming != null)
            {
                foreach (var keyword in incoming)
                {
                    List<Keyword> list = null;

                    if (!string.IsNullOrWhiteSpace(keyword.Type))
                    {
                        list = output.ContainsKey(keyword.Type) ? output[keyword.Type] : null;
                        if (list == null)
                        {
                            list = new List<Keyword>();
                            output[keyword.Type] = list;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(keyword.Thesaurus))
                    {
                        list = output.ContainsKey(keyword.Thesaurus) ? output[keyword.Thesaurus] : null;
                        if (list == null)
                        {
                            list = new List<Keyword>();
                            output[keyword.Thesaurus] = list;
                        }
                    }
                    else
                    {
                        list = output.ContainsKey("other") ? output["other"] : null;
                        if (list == null)
                        {
                            list = new List<Keyword>();
                            output["other"] = list;
                        }
                    }
                    
                    list.Add(new Keyword
                    {
                        Value = keyword.Keyword,
                        Thesaurus = keyword.Thesaurus,
                        Type = keyword.Type
                    });
                }
            }

            return output;
        }
    }


    public class Thumbnail
    {
        public string URL { get; set; }
        public string Type { get; set; }

        public static List<Thumbnail> CreateFromList(List<SimpleThumbnail> input)
        {
            List<Thumbnail> thumbnails = new List<Thumbnail>();

            foreach (var simpleThumbnail in input)
            {
                thumbnails.Add(new Thumbnail
                {
                    Type = simpleThumbnail.Type,
                    URL = simpleThumbnail.URL
                });
            }

            return thumbnails;
        }

        internal static List<SimpleThumbnail> ToSimpleThumbnailList(List<Thumbnail> list)
        {
            List<SimpleThumbnail> output = new List<SimpleThumbnail>();

            if (list != null)
            {
                foreach (var item in list)
                {
                    output.Add(new SimpleThumbnail
                    {
                        Type = item.Type,
                        URL = item.URL
                    });
                }
            }

            return output;
        }
    }

    public class DistributionGroup
    {
        public string Organization { get; set; }
        public string Protocol { get; set; }
        public string Url { get; set; }

        public override bool Equals(object obj)
        {
            DistributionGroup other = obj as DistributionGroup;
            return other.Organization == Organization && other.Protocol == Protocol && other.Url == Url;
        }
        public override int GetHashCode()
        {
            int hash = 0;

            if (Organization != null)
                hash = hash + Organization.GetHashCode();

            if (Protocol != null)
                hash = hash + Protocol.GetHashCode();

            if (Url != null)
                hash = hash + Url.GetHashCode();

            return hash;
        }
    }

    public class Distribution
    {
        public SimpleDistribution Details { get; set; }
        public List<SimpleDistributionFormat> Formats { get; set; }
    }

}