using System.Runtime.CompilerServices;
using ExpressiveAnnotations.Attributes;
using GeoNorgeAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Resources;

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
            KeywordsOther = new List<string>();
            KeywordsEnglish = new Dictionary<string, string>();
            Thumbnails = new List<Thumbnail>();
        }
        
        public string Uuid { get; set; }
        public string HierarchyLevel { get; set; }
        
        [Required]
        [Display(Name = "Metadata_Title", ResourceType = typeof(UI))]
        public string Title { get; set; }
        
        [Required]
        [Display(Name = "Metadata_Purpose", ResourceType = typeof(UI))]
        public string Purpose { get; set; }

        [Required]
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
        public List<String> KeywordsOther { get; set; }
        public Dictionary<string, string> KeywordsEnglish { get; set; }

        public string LegendDescriptionUrl { get; set; }
        public string ProductPageUrl { get; set; }
        public string ProductSheetUrl { get; set; }
        public string ProductSpecificationUrl { get; set; }

        public List<Thumbnail> Thumbnails { get; set; }

        public string Status { get; set; }

        [RegularExpression(@"-?([0-9]+)(\.[0-9]+)?", ErrorMessageResourceName = "Metadata_BoundingBox_East_Invalid", ErrorMessageResourceType = typeof(UI))]
        public string BoundingBoxEast { get; set; }

        [RegularExpression(@"-?([0-9]+)(\.[0-9]+)?", ErrorMessageResourceName = "Metadata_BoundingBox_West_Invalid", ErrorMessageResourceType = typeof(UI))]
        public string BoundingBoxWest { get; set; }

        [RegularExpression(@"-?([0-9]+)(\.[0-9]+)?", ErrorMessageResourceName = "Metadata_BoundingBox_North_Invalid", ErrorMessageResourceType = typeof(UI))]
        public string BoundingBoxNorth { get; set; }

        [RegularExpression(@"-?([0-9]+)(\.[0-9]+)?", ErrorMessageResourceName = "Metadata_BoundingBox_South_Invalid", ErrorMessageResourceType = typeof(UI))]
        public string BoundingBoxSouth { get; set; }

        /* dataset only */
        public string SupplementalDescription { get; set; }
        public string SpecificUsage { get; set; }  // bruksområde
        public string ResourceIdentifierName { get; set; }  // teknisk navn
        public string TopicCategory { get; set; }
        public string SpatialRepresentation { get; set; }

        public string DistributionFormatName { get; set; }
        public string DistributionFormatVersion { get; set; }
        public string DistributionUrl { get; set; }
        public string DistributionProtocol { get; set; }
        public string DistributionName { get; set; }

        public string ReferenceSystemCoordinateSystem { get; set; }
        public string ReferenceSystemNamespace { get; set; }
        
        // quality
        public string QualitySpecificationDate { get; set; }
        public string QualitySpecificationDateType { get; set; }
        public string QualitySpecificationExplanation { get; set; }
        public bool QualitySpecificationResult { get; set; }
        public string QualitySpecificationTitle { get; set; }
        public string ProcessHistory { get; set; }
        public string MaintenanceFrequency { get; set; }
        public string ResolutionScale { get; set; }

        // constraints
        public string UseLimitations { get; set; }
        public string AccessConstraints { get; set; }
        public string UseConstraints { get; set; }
        public string OtherConstraints { get; set; }
        public string SecurityConstraints { get; set; }

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

        public string EnglishTitle { get; set; }
        public string EnglishAbstract { get; set; }

        public string EnglishContactMetadataOrganization { get; set; }
        public string EnglishContactPublisherOrganization { get; set; }
        public string EnglishContactOwnerOrganization { get; set; }

        internal void FixThumbnailUrls()
        {
            foreach (var thumbnail in Thumbnails)
            {
                if (!thumbnail.URL.StartsWith("http"))
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
            allKeywords.AddRange(CreateKeywords(KeywordsInspire, "Inspire", null, SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1));
            allKeywords.AddRange(CreateKeywords(KeywordsNationalInitiative, "NationalInitiative", null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE));
            allKeywords.AddRange(CreateKeywords(KeywordsServiceTaxonomy, "Service", null, SimpleKeyword.THESAURUS_SERVICES_TAXONOMY));
            allKeywords.AddRange(CreateKeywords(KeywordsPlace, "Place", SimpleKeyword.TYPE_PLACE, null));
            allKeywords.AddRange(CreateKeywords(KeywordsTheme, "Theme", SimpleKeyword.TYPE_THEME, null));
            allKeywords.AddRange(CreateKeywords(KeywordsOther, "Other", null, null));
            return allKeywords;
        }

        internal bool HasAccess(string organization)
        {
            bool hasAccess = false;

            if (ContactMetadata != null && ContactMetadata.Organization != null && ContactMetadata.Organization.Equals(organization))
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

    }

    public class Contact
    {
        [Required(ErrorMessageResourceName = "Metadata_Contact_Name_Required", ErrorMessageResourceType = typeof(UI))]
        [Display(Name = "Contact_Name", ResourceType = typeof(UI))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceName = "Metadata_Contact_Organization_Required", ErrorMessageResourceType = typeof(UI))]
        [Display(Name = "Contact_Organization", ResourceType = typeof(UI))]
        public string Organization { get; set; }

        [Required(ErrorMessageResourceName = "Metadata_Contact_Email_Required", ErrorMessageResourceType = typeof(UI))]
        [Display(Name = "Contact_Email", ResourceType = typeof(UI))]
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

}