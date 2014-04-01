using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class ServiceLayerViewModel
    {
        public MetadataViewModel Metadata { get; set; }
        public List<WmsLayerViewModel> Layers { get; set; }
    }
}