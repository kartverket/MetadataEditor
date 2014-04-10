using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class WmsLayerViewModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }
        public List<string> Keywords { get; set; }
        public string BoundingBoxEast { get; set; }
        public string BoundingBoxWest { get; set; }
        public string BoundingBoxNorth { get; set; }
        public string BoundingBoxSouth { get; set; }
        public string Uuid { get; set; }
    }
}