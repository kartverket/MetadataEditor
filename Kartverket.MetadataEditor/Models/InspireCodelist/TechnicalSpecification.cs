using Newtonsoft.Json.Linq;
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
        private static List<TechnicalSpecification> _technicalSpesifications = GetCodeList();

        private static List<TechnicalSpecification> GetCodeList()
        {
            List<TechnicalSpecification> technicalSpecifications = new List<TechnicalSpecification>();

            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/metadata-kodelister/kartverket/tjenestestandarder";
            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(url);
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var codeList = response["containeditems"];

            foreach (var code in codeList)
            {
                JToken codevalueToken = code["codevalue"];
                string codeValue = codevalueToken?.ToString();
                string name = code["label"].ToString();
                DateTime validFromDate = code["ValidFrom"].ToObject<DateTime>();
                string validFrom = validFromDate.ToString("yyyy-MM-dd");

                technicalSpecifications.Add(new TechnicalSpecification { Name = name, Url = codeValue, PublicationDate = validFrom });
            }

            return technicalSpecifications;
        }

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