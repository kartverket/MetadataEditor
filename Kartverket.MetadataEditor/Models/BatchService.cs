using GeoNorgeAPI;
using Kartverket.MetadataEditor.Controllers;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class BatchService
    {
        private MetadataService _metadataService;
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        public BatchService() 
        {
            _metadataService = new MetadataService();
        }

        public void Update(BatchData data, string username)
        {
            try
            {
                foreach (var md in data.MetaData)
                {
                    MetadataViewModel metadata = _metadataService.GetMetadataModel(md.Uuid);
                    if (data.dataField == "ContactMetadata_Email")
                    {
                        metadata.ContactMetadata.Email = data.dataValue;
                    }

                    _metadataService.SaveMetadataModel(metadata, username);
                    
                }
              
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

        }
    }

    public class BatchData
    {
        public BatchData()
        {
            MetaData = new List<MetaDataEntry>();
        }
        public string dataField { get; set; }
        public string dataValue { get; set; }
        public List<MetaDataEntry> MetaData { get; set; }
    }
}