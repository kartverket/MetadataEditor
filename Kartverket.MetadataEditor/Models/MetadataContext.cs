using Kartverket.MetadataEditor.Models.OpenData;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class MetadataContext : DbContext
    {
        public MetadataContext() : base("DefaultConnection")
        { }
        public virtual DbSet<OpenMetadataEndpoint> OpenMetadataEndpoints { get; set; }
        public virtual DbSet<MetaDataEntry> MetaDataEntries { get; set; }
        
    }
}