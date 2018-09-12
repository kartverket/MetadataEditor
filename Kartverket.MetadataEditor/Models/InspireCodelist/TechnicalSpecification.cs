using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models.InspireCodelist
{
    public class TechnicalSpecification
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string PublicationDate { get; set; }
    }

    public class Technical
    {
        private static List<TechnicalSpecification> _technicalSpesifications = new List<TechnicalSpecification>()
        {
             new TechnicalSpecification
                {
                    Name = "EN ISO 19128:2005(E): Geographic information — Web map server interface",
                    Url = "http://www.iso.org/iso/iso_catalogue/catalogue_tc/catalogue_detail.htm?csnumber=32546",
                    PublicationDate = "2005-12-01"
                }
        };

        public static List<TechnicalSpecification> GetSpecifications
        {
            get
            {
                return _technicalSpesifications;
            }
            set {  }
        }

        public static TechnicalSpecification GetSpecification(string name)
        {
            return _technicalSpesifications.Where(s => s.Name == name).FirstOrDefault();
        }
    }
}