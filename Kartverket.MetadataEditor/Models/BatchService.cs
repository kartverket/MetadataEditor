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

        OfficeOpenXml.ExcelPackage excelPackage;
        OfficeOpenXml.ExcelWorksheet workSheet;

        public void Update(HttpPostedFileBase file, string username, string metadatafield)
        {
            excelPackage = new OfficeOpenXml.ExcelPackage();
            excelPackage.Load(file.InputStream);
            workSheet = excelPackage.Workbook.Worksheets[1];

            if (metadatafield == "AccessConstraints")
                UpdateRestriction(username, metadatafield);
            else if (metadatafield == "KeywordsNationalTheme")
                UpdateTheme(username, metadatafield);
            else if (metadatafield == "KeywordsNationalInitiative")
                UpdateKeywordsNationalInitiative(username, metadatafield);
            else if (metadatafield == "KeywordsInspire")
                UpdateKeywordsInspire(username, metadatafield);
            else if (metadatafield == "DistributionFormats")
                UpdateDistributionFormats(username, metadatafield);

            excelPackage.Dispose();

        }

        void UpdateTheme(string username, string metadatafield)
        {
            Dictionary<string, string> listOfAllowedNationalThemes = GetCodeList("42CECF70-0359-49E6-B8FF-0D6D52EBC73F");

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
                        SaveTheme(uuid, theme, username);
                }
            }
        }

        void UpdateRestriction(string username, string metadatafield)
        {
            Dictionary<string, string> listOfAllowedRestrictionValues = GetCodeList("2BBCD2DF-C943-4D22-8E49-77D434C8A80D");

            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;

            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var restriction = workSheet.Cells[row, 2].Text;
                var uuid = workSheet.Cells[row, 1].Text;

                if (!string.IsNullOrWhiteSpace(restriction) && !string.IsNullOrWhiteSpace(uuid))
                {

                    if (listOfAllowedRestrictionValues.ContainsKey(restriction))
                        SaveRestriction(uuid, restriction, username);
                }
            }
        }


        void UpdateKeywordsNationalInitiative(string username, string metadatafield)
        {
            Dictionary<string, string> listOfAllowedKeywordsNationalInitiative = GetCodeList("37204b11-4802-44b6-80a1-519968bd072f");

            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;
            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var uuidDelete = workSheet.Cells[row, 1].Text;
                RemoveKeywordNationalInitiative(uuidDelete, username);
            }
            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var keywordNationalInitiative = workSheet.Cells[row, 2].Text;
                var uuid = workSheet.Cells[row, 1].Text;
                var quality = workSheet.Cells[row, 3].Text;
                var qualityExplain = workSheet.Cells[row, 4].Text;

                if (!string.IsNullOrWhiteSpace(keywordNationalInitiative) && !string.IsNullOrWhiteSpace(uuid))
                {
                    if (listOfAllowedKeywordsNationalInitiative.ContainsKey(keywordNationalInitiative))
                        SaveKeywordNationalInitiative(uuid, keywordNationalInitiative, quality, qualityExplain, username);
                }
            }
        }

        void UpdateKeywordsInspire(string username, string metadatafield)
        {
            Dictionary<string, string> listOfAllowedKeywordsInspire = GetCodeList("e7e48bc6-47c6-4e37-be12-08fb9b2fede6");

            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;
            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var uuidDelete = workSheet.Cells[row, 1].Text;
                RemoveKeywordInspire(uuidDelete, username);
            }
            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var keywordInspire = workSheet.Cells[row, 2].Text;
                var uuid = workSheet.Cells[row, 1].Text;

                if (!string.IsNullOrWhiteSpace(keywordInspire) && !string.IsNullOrWhiteSpace(uuid))
                {
                    if (listOfAllowedKeywordsInspire.ContainsKey(keywordInspire))
                        SaveKeywordInspire(uuid, keywordInspire, username);
                }
            }
        }

        void UpdateDistributionFormats(string username, string metadatafield)
        {
            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;
            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var uuidDelete = workSheet.Cells[row, 1].Text;
                RemoveDistributionformats(uuidDelete, username);
            }
            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var format = workSheet.Cells[row, 2].Text;
                var uuid = workSheet.Cells[row, 1].Text;
                var version = workSheet.Cells[row, 3].Text;

                if (!string.IsNullOrWhiteSpace(format) && !string.IsNullOrWhiteSpace(uuid))
                {
                    SaveDistributionformats(uuid, format, version, username);
                }
            }
        }

        void SaveRestriction(string uuid, string restriction, string username)
        {
            try
            { 
            MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);

            metadata.AccessConstraints = restriction;    

            _metadataService.SaveMetadataModel(metadata, username);

            Log.Info("Batch update uuid: " + uuid + ", AccessConstraints: " + restriction);

            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid ,ex);
            }
        }

        void SaveTheme(string uuid, string theme, string username)
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
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        void SaveKeywordNationalInitiative(string uuid, string keyword, string quality,string qualityExplain, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
                metadata.KeywordsNationalInitiative.Add(keyword);
                if(keyword == "Inspire" && !string.IsNullOrEmpty(quality))
                {
                    metadata.QualitySpecificationDateInspire = new DateTime(2010, 12, 8 );
                    metadata.QualitySpecificationDateTypeInspire = "publication";
                    string explain = (quality == "godkjent" ? "Dataene er produsert iht produktspesifikasjonen" : qualityExplain);
                    if (string.IsNullOrEmpty(explain))
                        explain = metadata.QualitySpecificationExplanationInspire;
                    if (string.IsNullOrEmpty(explain))
                        explain = "Dataene er ikke produsert iht produktspesifikasjonen";
                    metadata.QualitySpecificationExplanationInspire = explain;
                    metadata.QualitySpecificationResultInspire = (quality == "godkjent" ? true : false);
                    metadata.QualitySpecificationTitleInspire = "Inspire";

                }
                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch update uuid: " + uuid + ", KeywordsNationalInitiative: " + keyword);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        void SaveKeywordInspire(string uuid, string keyword, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
                metadata.KeywordsInspire.Add(keyword);
                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch update uuid: " + uuid + ", KeywordsInspire: " + keyword);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        void SaveDistributionformats(string uuid, string format, string version, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
                if(metadata.DistributionFormats.Count == 1 && string.IsNullOrEmpty(metadata.DistributionFormats[0].Name))
                {
                    metadata.DistributionFormats[0].Name = format;
                    metadata.DistributionFormats[0].Version = version;
                }
                else
                metadata.DistributionFormats.Add(new SimpleDistributionFormat { Name = format, Version = version });
                
                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch update uuid: " + uuid + ", distributionformat: " + format);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        void RemoveKeywordNationalInitiative(string uuid, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
                metadata.KeywordsNationalInitiative = null;
                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch remove KeywordsNationalTheme, uuid: " + uuid );
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        void RemoveKeywordInspire(string uuid, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
                metadata.KeywordsInspire = null;
                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch remove KeywordsInspire, uuid: " + uuid);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        void RemoveDistributionformats(string uuid, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
                metadata.DistributionFormats = new List<SimpleDistributionFormat>();
                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch remove Distributionformats, uuid: " + uuid);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
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