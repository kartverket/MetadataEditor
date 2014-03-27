using GeoNorgeAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class MetadataViewModel
    {
        
        
        public string Uuid { get; set; }
        public string HierarchyLevel { get; set; }
        public string Title { get; set; }
        public string Purpose { get; set; }
        public string Abstract { get; set; }

        public Contact ContactPublisher { get; set; }
        public Contact ContactPointOfContact { get; set; }
        public Contact ContactAuthor { get; set; }
        public List<Contact> ContactOthers { get; set; }

        public Dictionary<string, List<Keyword>> Keywords { get; set; }

        public List<Keyword> KeywordsTheme { get; set; }
        public List<Keyword> KeywordsPlace { get; set; }
        public List<Keyword> KeywordsInspire { get; set; }
        public List<Keyword> KeywordsServiceTaxonomy { get; set; }
        public List<Keyword> KeywordsNationalInitiative { get; set; }

        public string LegendDescriptionUrl { get; set; }
        public string ProductPageUrl { get; set; }
        public string ProductSheetUrl { get; set; }
        public string ProductSpecificationUrl { get; set; }

        public List<Thumbnail> Thumbnails { get; set; }

        public string Status { get; set; }

        public string BoundingBoxEast { get; set; }
        public string BoundingBoxWest { get; set; }
        public string BoundingBoxNorth { get; set; }
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

        public DateTime? DateCreated { get; set; }
        public DateTime? DatePublished { get; set; }
        public DateTime? DateUpdated { get; set; }

        public DateTime? DateMetadataUpdated { get; set; }

        internal void FixThumbnailUrls()
        {
            foreach (var thumbnail in Thumbnails)
            {
                if (!thumbnail.URL.StartsWith("http"))
                {
                    thumbnail.URL = "https://www.geonorge.no/geonetworkbeta/srv/eng/resources.get?uuid=" + Uuid + "&access=public&fname=" + thumbnail.URL;
                }
            }
        }       
    }

    public class Contact
    {
        public string Name { get; set; }
        public string Organization { get; set; }
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

        internal static List<Keyword> FilterKeywords(List<SimpleKeyword> allKeywords, string type, string thesaurus)
        {
            List<Keyword> filteredList = new List<Keyword>();

            bool filterOnType = !string.IsNullOrWhiteSpace(type);
            bool filterOnThesaurus = !string.IsNullOrWhiteSpace(thesaurus);

            foreach (SimpleKeyword simpleKeyword in allKeywords)
            {
                if (filterOnType && simpleKeyword.Type.Equals(type))
                {
                    filteredList.Add(new Keyword(simpleKeyword));
                }
                else if (filterOnThesaurus && simpleKeyword.Thesaurus.Equals(thesaurus))
                {
                    filteredList.Add(new Keyword(simpleKeyword));
                }
                else if (!filterOnType && !filterOnThesaurus)
                {
                    filteredList.Add(new Keyword(simpleKeyword));
                }
            }
            return filteredList;
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
    }

}