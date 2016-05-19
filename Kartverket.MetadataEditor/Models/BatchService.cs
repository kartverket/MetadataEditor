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
                if (!string.IsNullOrWhiteSpace(data.dataField) && !string.IsNullOrWhiteSpace(data.dataValue)) 
                {

                    foreach (var md in data.MetaData)
                    {
                        bool metadataUpdated = false;

                        MetadataViewModel metadata = _metadataService.GetMetadataModel(md.Uuid);

                        //Metadatakontakt
                        if (data.dataField == "ContactMetadata_Organization")
                        {
                            metadata.ContactMetadata.Organization = data.dataValue;
                            metadataUpdated = true;
                        }
                        else if (data.dataField == "ContactMetadata_Name")
                        {
                            metadata.ContactMetadata.Name = data.dataValue;
                            metadataUpdated = true;
                        }
                        else if (data.dataField == "ContactMetadata_Email")
                        {
                            metadata.ContactMetadata.Email = data.dataValue;
                            metadataUpdated = true;
                        }
                        //Teknisk kontakt
                        else if (data.dataField == "ContactPublisher_Organization")
                        {
                            metadata.ContactPublisher.Organization = data.dataValue;
                            metadataUpdated = true;
                        }
                        else if (data.dataField == "ContactPublisher_Name")
                        {
                            metadata.ContactPublisher.Name = data.dataValue;
                            metadataUpdated = true;
                        }
                        else if (data.dataField == "ContactPublisher_Email")
                        {
                            metadata.ContactPublisher.Email = data.dataValue;
                            metadataUpdated = true;
                        }
                        //Faglig kontakt
                        else if (data.dataField == "ContactOwner_Organization")
                        {
                            metadata.ContactOwner.Organization = data.dataValue;
                            metadataUpdated = true;
                        }
                        else if (data.dataField == "ContactOwner_Name")
                        {
                            metadata.ContactOwner.Name = data.dataValue;
                            metadataUpdated = true;
                        }
                        else if (data.dataField == "ContactOwner_Email")
                        {
                            metadata.ContactOwner.Email = data.dataValue;
                            metadataUpdated = true;
                        }
                        //Oppdateringshyppighet
                        else if (data.dataField == "MaintenanceFrequency")
                        {
                            metadata.MaintenanceFrequency = data.dataValue;
                            metadataUpdated = true;
                        }

                        if (metadataUpdated) 
                        { 
                        _metadataService.SaveMetadataModel(metadata, username);
                        Log.Info("Batch update uuid: " + metadata.Uuid + ", " + data.dataField + ": " + data.dataValue);
                        }
                    }

                }
              
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

        }

        public void Update(HttpPostedFileBase file, string username)
        {
            Dictionary<string, string> listOfAllowedNationalThemes =  GetCodeList("42CECF70-0359-49E6-B8FF-0D6D52EBC73F");

            var excelPackage = new OfficeOpenXml.ExcelPackage();
            excelPackage.Load(file.InputStream);
            var workSheet = excelPackage.Workbook.Worksheets[1];

            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;

                for (int row = start.Row + 1; row <= end.Row; row++)
                { 
                    var theme = workSheet.Cells[row, 2].Text;
                    var uuid = workSheet.Cells[row, 1].Text; 

                    if (!string.IsNullOrWhiteSpace(theme) && !string.IsNullOrWhiteSpace(uuid))
                    {
                    var key = new KeyValuePair<string, string>(theme, theme);

                    if (listOfAllowedNationalThemes.Contains(key))
                        UpdateTheme(uuid, theme, username);
                    }
                } 

            }

        void UpdateTheme(string uuid, string theme, string username)
        {
            try
            { 
            MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
            var keyword = new List<string>();
            keyword.Add(theme);
            metadata.KeywordsNationalTheme = keyword;
            _metadataService.SaveMetadataModel(metadata, username);

            Log.Info("Batch update uuid: " + uuid + ", KeywordsNationalTheme: " + theme);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid ,ex);
            }
        }

        public void UpdateAll(BatchData data, string username, string organization)
        {
            try
            {
                MetadataIndexViewModel model = new MetadataIndexViewModel();
                int offset = 1;
                int limit = 50;
                model = _metadataService.SearchMetadata(organization, "", offset, limit);
                model.UserOrganization = organization;

                foreach (var item in model.MetadataItems)
                {
                    data.MetaData.Add(new MetaDataEntry { Uuid = item.Uuid });
                }

                Update(data, username);

                int numberOfRecordsMatched = model.TotalNumberOfRecords;
                int next = model.OffsetNext();

                while (next < numberOfRecordsMatched)
                {
                    data.MetaData = null; data.MetaData = new List<MetaDataEntry>();

                    model = _metadataService.SearchMetadata(organization, "", next, limit);
                    model.UserOrganization = organization;

                    foreach (var item in model.MetadataItems)
                    {
                        data.MetaData.Add(new MetaDataEntry { Uuid = item.Uuid });
                    }

                    Update(data, username);

                    next = model.OffsetNext();
                    if (next == 0) break;
                }

 
            }

            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }


        public Dictionary<string, string> GetCodeList(string systemid)
        {
            Dictionary<string, string> CodeValues = new Dictionary<string, string>();
            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/kodelister/" + systemid;
            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(url);
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var codeList = response["containeditems"];

            foreach (var code in codeList)
            {
                var codevalue = code["codevalue"].ToString();
                if (string.IsNullOrWhiteSpace(codevalue))
                    codevalue = code["label"].ToString();

                if (!CodeValues.ContainsKey(codevalue))
                {
                    CodeValues.Add(codevalue, code["label"].ToString());
                }
            }

            CodeValues = CodeValues.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

            return CodeValues;
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