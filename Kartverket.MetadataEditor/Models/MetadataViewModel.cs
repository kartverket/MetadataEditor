using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class MetadataViewModel
    {
        public string Uuid { get; set; }
        public string HierarchyLevel { get; set; }
        public string Title { get; set; }
        public string Purpose { get; set; }
        public string Abstract { get; set; }
        
        public string Usage { get; set; }


        /* dataset only */
        public string SupplementalDescription { get; set; }

    }
}