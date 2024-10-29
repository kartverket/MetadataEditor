using System.Runtime.CompilerServices;
using ExpressiveAnnotations.Attributes;
using GeoNorgeAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kartverket.MetadataEditor.Helpers;
using Resources;

namespace Kartverket.MetadataEditor.Models
{
    public class SimpleMetadataViewModel
    {

        public SimpleMetadataViewModel()
        {
            KeywordsPlace = new List<string>();
            KeywordsNationalInitiative = new List<string>();
            KeywordsNationalTheme = new List<string>();
            KeywordsEnglish = new Dictionary<string, string>();
        }

        public string Uuid { get; set; }
        public string HierarchyLevel { get; set; }
        public string ParentIdentifier { get; set; }
        public string MetadataStandard { get; set; }

        [Required(ErrorMessage = null)]
        [Display(Name = "SimpleMetadata_Title", ResourceType = typeof(UI))]
        public string Title { get; set; }
        public string TitleFromSelectedLanguage { get; set; }

        [Required(ErrorMessage = "Sammendrag er påkrevd")]
        [Display(Name = "Metadata_Abstract", ResourceType = typeof(UI))]
        public string Abstract { get; set; }

        public Contact ContactMetadata { get; set; }
        public Contact ContactPublisher { get; set; }
        public Contact ContactOwner { get; set; }

        [Required(ErrorMessage = "Geografisk utstrekning nord er påkrevd")]
        [RegularExpression(@"-?([0-9]+)(\.[0-9]+)?", ErrorMessageResourceName = "Metadata_BoundingBox_East_Invalid", ErrorMessageResourceType = typeof(UI), ErrorMessage = null)]
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

        public string SupplementalDescription { get; set; }
        public string SpecificUsage { get; set; }  // bruksområde

        public string ProcessHistory { get; set; }

        public string ProductPageUrl { get; set; }
        public string LegendDescriptionUrl { get; set; }

        [Display(Name = "Metadata_DateUpdated", ResourceType = typeof(UI))]
        [DisplayFormat(NullDisplayText = "", ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [Required]
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateMetadataUpdated { get; set; }

        public string DistributionProtocol { get; set; }
        [RequiredIf("IsDokDataset()", ErrorMessage = "URL til nedlastingsside er påkrevd for DOK-datasett")]
        public string DistributionUrl { get; set; }

        [Required(ErrorMessage = "Oppdateringshyppighet er påkrevd")]
        public string MaintenanceFrequency { get; set; }

        [MustHaveOneElementAttribute(ErrorMessage = "Geografisk område er påkrevd")]
        public List<String> KeywordsPlace { get; set; }
        public List<String> KeywordsNationalInitiative { get; set; }
        [MustHaveOneElementAttribute(ErrorMessage = "Nasjonal temakategori er påkrevd")]
        public List<String> KeywordsNationalTheme { get; set; }
        public Dictionary<string, string> KeywordsEnglish { get; set; }

        public string UseConstraints { get; set; }
        public string OtherConstraintsLink { get; set; }
        public string OtherConstraintsLinkText { get; set; }
        public string OtherConstraintsAccess { get; set; }

        public string Published { get; set; }

        public string EnglishTitle { get; set; }
        public string EnglishAbstract { get; set; }

        public string EnglishContactMetadataOrganization { get; set; }
        public string EnglishContactPublisherOrganization { get; set; }
        public string EnglishContactOwnerOrganization { get; set; }


        public bool IsDokDataset()
        {
            return KeywordsNationalInitiative.Contains("Det offentlige kartgrunnlaget");
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


        public string GetInnholdstypeCSS()
        {
            string t = "label-default";
            if (HierarchyLevel == "dataset") t = "label-datasett";
            else if (HierarchyLevel == "software") t = "label-applikasjon";
            else if (HierarchyLevel == "service" && (!string.IsNullOrWhiteSpace(ParentIdentifier))) t = "label-tjenestelag";
            else if (HierarchyLevel == "service") t = "label-tjeneste";
            else if (HierarchyLevel == "series") t = "label-datasettserie";

            return t;
        }

        public string GetInnholdstype()
        {
            string t = HierarchyLevel;
            if (HierarchyLevel == "dataset") t = "Datasett";
            else if (HierarchyLevel == "software") t = "Programvare";
            else if (HierarchyLevel == "service" && (!string.IsNullOrWhiteSpace(ParentIdentifier)) && DistributionProtocol != null && DistributionProtocol.Contains("WFS")) t = "WFS-lag";
            else if (HierarchyLevel == "service" && (!string.IsNullOrWhiteSpace(ParentIdentifier))) t = "WMS-lag";
            else if (HierarchyLevel == "service") t = "Tjeneste";
            else if (HierarchyLevel == "series") t = "Datasettserie";

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

        internal List<SimpleKeyword> CreateKeywords(List<string> inputList, string prefix, string type = null, string thesaurus = null)
        {
            List<SimpleKeyword> output = new List<SimpleKeyword>();
            if (inputList != null)
            {
                foreach (var keyword in inputList)
                {
                    string englishTranslation = null;
                    string keyForEnglishTranslation = prefix + "_" + keyword;
                    if (KeywordsEnglish != null && KeywordsEnglish.ContainsKey(keyForEnglishTranslation))
                    {
                        englishTranslation = KeywordsEnglish[keyForEnglishTranslation];
                    }

                    output.Add(new SimpleKeyword
                    {
                        Keyword = keyword,
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
            allKeywords.AddRange(CreateKeywords(KeywordsNationalInitiative, "NationalInitiative", null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE));
            allKeywords.AddRange(CreateKeywords(KeywordsNationalTheme, "NationalTheme", null, SimpleKeyword.THESAURUS_NATIONAL_THEME));
            allKeywords.AddRange(CreateKeywords(KeywordsPlace, "Place", SimpleKeyword.TYPE_PLACE, null));
            return allKeywords;
        }

        public string TitleTranslated()
        {
            return CultureHelper.IsNorwegian()
                ? Title
                : (!string.IsNullOrWhiteSpace(EnglishTitle) ? EnglishTitle : Title);
        }

    }

}