using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class KomDataService
    {
        Dictionary<string, string> fylker;

        public KomDataService()
        {
            fylker = GetFylkesList();
        }

        public List<string> GetPlaces(string nord, string aust)
        {
            var url = "https://api.kartverket.no/kommuneinfo/v1/punkt?nord="+ nord + "&ost="+ aust + "&koordsys=4258";

            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(url);
            var areaInfo = Newtonsoft.Json.Linq.JObject.Parse(data);

            var fylkesnavn = areaInfo["fylkesnavn"].ToString();
            var fylkesnummer = areaInfo["fylkesnummer"].ToString();
            var kommunenavn = areaInfo["kommunenavn"].ToString() + " kommune";
            var kommunenummer = areaInfo["kommunenummer"].ToString();

            List<string> places = new List<string>();


            var fylke = fylkesnavn;

                if (!string.IsNullOrEmpty(fylke) && !places.Contains(fylke))
                    places.Add(fylke);

                if (!places.Contains(kommunenavn))
                    places.Add(kommunenavn);

            

            return places;

        }

        Dictionary<string,string> GetFylkesList()
        {
            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/sosi-kodelister/kartverket/fylkesnummer";
            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(url);
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var codeList = response["containeditems"];

            Dictionary<string, string> fylks = new Dictionary<string, string>();

            foreach (var code in codeList)
            {
                if(!fylks.ContainsKey(code["codevalue"].ToString()))
                    fylks.Add(code["codevalue"].ToString(), code["label"].ToString());               
            }

            return fylks;
        }

        public List<MunicipalityViewModel> GetListOfMunicipalityOrganizations()
        {
            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue("municipalityorganizations");

            List<MunicipalityViewModel> Organizations = new List<MunicipalityViewModel>();

            if (cache != null)
            {
                Organizations = cache as List<MunicipalityViewModel>;
            }

            if (Organizations.Count < 1)
            {
                System.Net.WebClient c = new System.Net.WebClient();
                c.Encoding = System.Text.Encoding.UTF8;
                var json = c.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/v2/organisasjoner/kommuner");
                dynamic result = Newtonsoft.Json.Linq.JArray.Parse(json);

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        Organizations.Add(
                            new MunicipalityViewModel
                            {
                                Name = item.Name,
                                OrganizationNumber = item.OrganizationNumber,
                                MunicipalityCode = item.MunicipalityCode,
                                BoundingBoxEast = item.BoundingBoxEast,
                                BoundingBoxNorth = item.BoundingBoxNorth,
                                BoundingBoxSouth = item.BoundingBoxSouth,
                                BoundingBoxWest = item.BoundingBoxWest,
                                GeographicCenterX = item.GeographicCenterX,
                                GeographicCenterY = item.GeographicCenterY
                            });
                    }
                }

                Organizations = Organizations.OrderBy(o => o.Name).ToList();
                memCacher.Set("municipalityorganizations", Organizations, new DateTimeOffset(DateTime.Now.AddYears(1)));
            }

            return Organizations;
        }

    }
   
}