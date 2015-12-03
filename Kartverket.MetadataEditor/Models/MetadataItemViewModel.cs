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

        public string MetadataViewParameters()
        {
            var seoUrl = new SeoUrl(Organization, Title);
            return "Metadata/"+ seoUrl.Title + "/" + Uuid;
        }

        public string GetInnholdstypeCSS()
        {
            string t = "label-default";
            if (Type == "dataset") t = "label-datasett";
            else if (Type == "software") t = "label-applikasjon";
            else if (Type == "service" && (!string.IsNullOrWhiteSpace(Relation))) t = "label-tjenestelag";
            else if (Type == "service") t = "label-tjeneste";
            else if (Type == "series") t = "label-datasettserie";

            return t;
        }

        public string GetInnholdstype()
        {
            string t = Type;
            if (Type == "dataset") t = "Datasett";
            else if (Type == "software") t = "Programvare";
            else if (Type == "service" && (!string.IsNullOrWhiteSpace(Relation))) t = "WMS-lag";
            else if (Type == "service") t = "Tjeneste";
            else if (Type == "series") t = "Datasettserie";

            return t;
        }

    }

}