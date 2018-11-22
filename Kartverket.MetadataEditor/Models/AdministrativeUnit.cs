using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models.Rdf
{
    public class AdministrativeUnit
    {
        public Head head { get; set; }
        public Results results { get; set; }
    }

    public class Head
    {
        public object[] link { get; set; }
        public string[] vars { get; set; }
    }

    public class Results
    {
        public bool distinct { get; set; }
        public bool ordered { get; set; }
        public Binding[] bindings { get; set; }
    }

    public class Binding
    {
        public Uri uri { get; set; }
        public Enh_Navn enh_navn { get; set; }
        public Enh_Type enh_type { get; set; }
        public Upper_Enh upper_enh { get; set; }
    }

    public class Uri
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Enh_Navn
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Enh_Type
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Upper_Enh
    {
        public string type { get; set; }
        public string value { get; set; }
    }

}