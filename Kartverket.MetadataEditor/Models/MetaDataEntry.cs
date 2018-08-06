using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class MetaDataEntry
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string OrganizationName { get; set; }
        public string ContactEmail { get; set; }
        public string Status { get; set; }
        public virtual List<Error> Errors { get; set; }
    }

    public class Error
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Message { get; set; }
    }
}