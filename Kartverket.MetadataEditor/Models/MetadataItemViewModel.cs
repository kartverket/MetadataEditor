using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class MetadataItemViewModel
    {
        public MetadataItemViewModel() 
        {
            Relations = new List<MetadataItemViewModel>();
        }

        public string Uuid { get; set; }
        public string Title { get; set; }
        public string Organization { get; set; }
        public string Type { get; set; }

        public List<MetadataItemViewModel> Relations { get; set; }
    }
}