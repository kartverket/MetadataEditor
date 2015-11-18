using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kartverket.MetadataEditor.no.geonorge.ws;

namespace Kartverket.MetadataEditor.Models
{
    public class KomDataService
    {
        SokKomDataClient result;

        System.Collections.Specialized.NameValueCollection settings = System.Web.Configuration.WebConfigurationManager.AppSettings;
        string username;
        string password;

        public KomDataService()
        {
            username = settings["WebserviceGeonorgeUsername"];
            password = settings["WebserviceGeonorgePassword"];

            result = new SokKomDataClient();
        }

        public List<string> GetPlaces(double nordMin, double austMin, double nordMax, double austMax,int koordSysUt, int koordSysInn)
        {
            //Todo fix coordinate search
            //result.kommuneSok1(username, password, null, 0 , "", 84, 84, 57, 2 , 72, 33); //Gir ikke resultat
            //result.kommuneSok1(username, password, null, 0, "", koordSysUt, koordSysInn, nordMin, austMin, nordMax, austMax);
            var kommuner = result.kommuneSok1(username, password, null, 0, "os*", 84, 0, 0, 0, 0, 0); // Gir resultat

            List<string> places = new List<string>();

            foreach (var kommune in kommuner.resultat)
            {
                var kommunenavn = kommune.kNavn;
                var kommunenr = kommune.kommNr;

                var fylke = GetFylke(kommunenr);

                if (!string.IsNullOrEmpty(fylke) && !places.Contains(fylke))
                    places.Add(fylke);

                if (!places.Contains(kommunenavn))
                    places.Add(kommunenavn);

            }

            return places;

        }

        private string GetFylke(int kommunenr)
        {
            string fylke = kommunenr.ToString("0000");
            fylke = fylke.Substring(0, 2);

            string fylkesnavn = "";

            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/sosi-kodelister/kartverket/fylkesnummer";
            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(url);
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var codeList = response["containeditems"];

            foreach (var code in codeList)
            {
                var codevalue = code["codevalue"].ToString();
                if (codevalue == fylke)
                {
                    fylkesnavn = code["label"].ToString();
                    break;
                }
            }

            return fylkesnavn;
        }
    }
   
}