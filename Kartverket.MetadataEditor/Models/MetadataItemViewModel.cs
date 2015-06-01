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
            if (Type == "dataset") t = "label-success";
            else if (Type == "software") t = "label-warning";
            else if (Type == "service" && (!string.IsNullOrWhiteSpace(Relation))) t = "label-info";
            else if (Type == "service") t = "label-info";
            else if (Type == "series") t = "label-primary";

            return t;
        }

        public string GetInnholdstype()
        {
            string t = Type;
            if (Type == "dataset") t = "Datasett";
            else if (Type == "software") t = "Programvare";
            else if (Type == "service" && (!string.IsNullOrWhiteSpace(Relation))) t = "WMS-lag (Tjenestelag)";
            else if (Type == "service") t = "Tjeneste";
            else if (Type == "series") t = "Datasettserie";

            return t;
        }

    }

}