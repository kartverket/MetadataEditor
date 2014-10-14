using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}