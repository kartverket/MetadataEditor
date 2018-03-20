using Kartverket.MetadataEditor.Helpers;
using Kartverket.MetadataEditor.Models.Translations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Kartverket.MetadataEditor.Models
{
    public class MetadataItemViewModel
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string Organization { get; set; }
        public string Type { get; set; }
        public string Relation { get; set; }
        public string GeoNetworkViewUrl { get; set; }
        public string GeoNetworkXmlDownloadUrl { get; set; }
        public string Uri { get; set; }
        public string UriProtocol { get; set; }
        public string UriName { get; set; }

    public string MetadataViewParameters()
        {
            var seoUrl = new SeoUrl(Organization, Title);
            return "Metadata/"+ seoUrl.Title + "/" + Uuid;
        }

        public string GetInnholdstypeCSS()
        {
            string t = "label-default";
            if (Type == "dataset") t = "label-datasett";
            else if (Type == "service" && (!string.IsNullOrWhiteSpace(UriProtocol) && !string.IsNullOrWhiteSpace(UriName))) t = "label-tjenestelag";
            else if (Type == "service") t = "label-tjeneste";
            else if (Type == "software") t = "label-applikasjon";
            else if (Type == "series") t = "label-datasettserie";
            else if (Type == "dimensionGroup") t = "label-datasett";
            return t;
        }

        public string GetInnholdstype()
        {
            if (CultureHelper.GetCurrentCulture() == Culture.NorwegianCode)
            {
                string t = Type;
                if (Type == "dataset") t = "Datasett";
                else if (Type == "service" && (!string.IsNullOrWhiteSpace(UriProtocol) && !string.IsNullOrWhiteSpace(UriName))) t = "Tjenestelag";
                else if (Type == "service") t = "Tjeneste";
                else if (Type == "software") t = "Applikasjon";
                else if (Type == "series") t = "Datasettserie";
                else if (Type == "dimensionGroup") t = "Datapakke";
                return t;
            }
            else
            {
                string t = Type;
                if (Type == "dataset") t = "Dataset";
                else if (Type == "service" && (!string.IsNullOrWhiteSpace(UriProtocol) && !string.IsNullOrWhiteSpace(UriName))) t = "Service layer";
                else if (Type == "service") t = "Service";
                else if (Type == "software") t = "Application";
                else if (Type == "series") t = "Dataset series";
                else if (Type == "dimensionGroup") t = "Data package";
                return t;
            }
        }

    }

}