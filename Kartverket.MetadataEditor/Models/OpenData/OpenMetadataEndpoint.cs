using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models.OpenData
{
    public class OpenMetadataEndpoint
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string OrganizationName { get; set; }

        public override string ToString()
        {
            return $"{OrganizationName} [url={Url}]";
        }
    }
}