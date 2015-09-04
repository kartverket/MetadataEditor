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

                    //Metadatakontakt
                    if (data.dataField == "ContactMetadata_Organization")
                    {
                        metadata.ContactMetadata.Organization = data.dataValue;
                    }
                    else if (data.dataField == "ContactMetadata_Name")
                    {
                        metadata.ContactMetadata.Name = data.dataValue;
                    }
                    else if (data.dataField == "ContactMetadata_Email")
                    {
                        metadata.ContactMetadata.Email = data.dataValue;
                    }
                    //Teknisk kontakt
                    else if (data.dataField == "ContactPublisher_Organization")
                    {
                        metadata.ContactPublisher.Organization = data.dataValue;
                    }
                    else if (data.dataField == "ContactPublisher_Name")
                    {
                        metadata.ContactPublisher.Name = data.dataValue;
                    }
                    else if (data.dataField == "ContactPublisher_Email")
                    {
                        metadata.ContactPublisher.Email = data.dataValue;
                    }
                    //Faglig kontakt
                    else if (data.dataField == "ContactOwner_Organization")
                    {
                        metadata.ContactOwner.Organization = data.dataValue;
                    }
                    else if (data.dataField == "ContactOwner_Name")
                    {
                        metadata.ContactOwner.Name = data.dataValue;
                    }
                    else if (data.dataField == "ContactOwner_Email")
                    {
                        metadata.ContactOwner.Email = data.dataValue;
                    }
                    //Oppdateringshyppighet
                    else if (data.dataField == "MaintenanceFrequency")
                    {
                        metadata.MaintenanceFrequency = data.dataValue;
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