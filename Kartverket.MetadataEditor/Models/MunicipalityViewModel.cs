using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class MunicipalityViewModel
    {
        public string Name { get; set; }
        public string OrganizationNumber { get; set; }
        public string MunicipalityCode { get; set; }
        public string GeographicCenterX { get; set; }
        public string GeographicCenterY { get; set; }
        public string BoundingBoxNorth { get; set; }
        public string BoundingBoxSouth { get; set; }
        public string BoundingBoxEast { get; set; }
        public string BoundingBoxWest { get; set; }
    }
}