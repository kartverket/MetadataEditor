using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class ServiceLayerViewModel
    {
        public MetadataViewModel Metadata { get; set; }

        [DefaultValue(typeof(List<WmsLayerViewModel>))]
        public List<WmsLayerViewModel> Layers { get; set; }

        public string WmsUrl { get; set; }

        public string GetCapabilitiesUrl()
        {
            return WmsUrl;
        }
    }
}