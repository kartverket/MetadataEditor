﻿using GeoNorgeAPI;
using Kartverket.MetadataEditor.Controllers;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Reflection;
using Kartverket.Geonorge.Utilities;
using Kartverket.MetadataEditor.Models.Translations;
using Newtonsoft.Json.Linq;
using www.opengis.net;
using Kartverket.MetadataEditor.Models.Rdf;
using GeoNetworkUtil = Kartverket.MetadataEditor.Util.GeoNetworkUtil;

namespace Kartverket.MetadataEditor.Models
{
    public class BatchService : IBatchService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IMetadataService _metadataService;
        string thumbnailFolder;

        private GeoNorge _geoNorge;
        private IAdministrativeUnitService _administrativeUnitService;

        public BatchService(IMetadataService metadataService, IAdministrativeUnitService administrativeUnitService)
        {
            _metadataService = metadataService;
            _administrativeUnitService = administrativeUnitService;
        }

        private void LogEventsDebug(string log)
        {

            Log.Debug(log);
        }

        private void LogEventsError(string log, Exception ex)
        {
            Log.Error(log, ex);
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

        Dictionary<string, string> inspireList;
        Dictionary<string, string> nationalThemeList;
        Dictionary<string, string> nationalInitiativeList;

        public void UpdateRegisterTranslations(string username, string uuid = null)
        {
            _metadataService.UpdateRegisterTranslations(username, uuid);
        }

        int NumberOfUpdatedKeywordPlaceUris = 0;

        public void UpdateKeywordPlaceUri(string username)
        {
            string searchString = "";
            System.Collections.Specialized.NameValueCollection settings = System.Web.Configuration.WebConfigurationManager.AppSettings;
            string server = settings["GeoNetworkUrl"];
            string usernameGeonetwork = settings["GeoNetworkUsername"];
            string password = settings["GeoNetworkPassword"];
            _geoNorge = new GeoNorgeAPI.GeoNorge(usernameGeonetwork, password, server);
            _geoNorge.OnLogEventDebug += new GeoNorgeAPI.LogEventHandlerDebug(LogEventsDebug);
            _geoNorge.OnLogEventError += new GeoNorgeAPI.LogEventHandlerError(LogEventsError);


            inspireList = GetCodeListEnglish("E7E48BC6-47C6-4E37-BE12-08FB9B2FEDE6");
            nationalThemeList = GetCodeListEnglish("42CECF70-0359-49E6-B8FF-0D6D52EBC73F");
            nationalInitiativeList = GetCodeListEnglish("37204B11-4802-44B6-80A1-519968BD072F");

            Log.Info("Start batch update keyword place URI");

            try
            {
                SearchResultsType model = null;
                int offset = 1;
                int limit = 50;
                model = _geoNorge.SearchIso(searchString, offset, limit, false);
                Log.Info("Running search from position:" + offset);
                foreach (var item in model.Items)
                {
                    var metadataItem = item as MD_Metadata_Type;
                    string identifier = metadataItem.fileIdentifier != null ? metadataItem.fileIdentifier.CharacterString : null;
                    UpdateAdminUnitUri(identifier, username);
                }

                int numberOfRecordsMatched = int.Parse(model.numberOfRecordsMatched);
                int next = int.Parse(model.nextRecord);

                while (next > 0 && next < numberOfRecordsMatched)
                {
                    Log.Info("Running search from position:" + next);
                    model = _geoNorge.SearchIso(searchString, next, limit, false);

                    foreach (var item in model.Items)
                    {
                        var metadataItem = item as MD_Metadata_Type;
                        string identifier = metadataItem.fileIdentifier != null ? metadataItem.fileIdentifier.CharacterString : null;
                        UpdateAdminUnitUri(identifier, username);
                    }

                    next = int.Parse(model.nextRecord);
                    if (next == 0) break;
                }

                Log.Info("Finished batch update keyword place URI, updated: " + NumberOfUpdatedKeywordPlaceUris + " of total:" + numberOfRecordsMatched + " metadata");
            }

            catch (Exception ex)
            {
                Log.Error("Error batch update stopped for batch update keyword place URI: " + ex.Message);
            }


        }

        private void UpdateAdminUnitUri(string uuid, string username)
        {
            try
            {
                Log.Info("Running batch update keyword place/administrative units URI for uuid: " + uuid);

                SimpleMetadata metadata = new SimpleMetadata(_geoNorge.GetRecordByUuid(uuid));
                MetadataViewModel model = _metadataService.GetMetadataModel(uuid);

                if (model.KeywordsPlace != null && model.KeywordsPlace.Count > 0)
                {
                    var administrativeUnits = model.KeywordsAdministrativeUnits;
                    var places = model.KeywordsPlace;
                    _administrativeUnitService.UpdateKeywordsAdministrativeUnitsWithUri(ref administrativeUnits, ref places);
                    model.KeywordsAdministrativeUnits = administrativeUnits;
                    model.KeywordsPlace = places;
                    metadata.Keywords = model.GetAllKeywords();

                    if (model.KeywordsAdministrativeUnits.Count > 0)
                    {

                        metadata.RemoveUnnecessaryElements();
                        var transaction = _geoNorge.MetadataUpdate(metadata.GetMetadata(), GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username));
                        if (transaction.TotalUpdated == "0")
                            Log.Error("No records updated batch update keyword place/administrative units URI uuid: " + uuid);

                        NumberOfUpdatedKeywordPlaceUris++;
                        Log.Info("Finished batch update keyword place/administrative units URI uuid: " + uuid);
                    }
                    else {
                        Log.Info("No keywords to update for administrative units uuid: " + uuid);
                    }
                }

            }

            catch (Exception ex)
            {
                Log.Error("Error batch update keyword place URI: " + ex.Message);
            }
        }

        public Dictionary<string, string> GetCodeListEnglish(string systemid)
        {
            Dictionary<string, string> CodeValues = new Dictionary<string, string>();

            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/kodelister/" + systemid;
            System.Net.WebClient c = new System.Net.WebClient();
            c.Headers.Remove("Accept-Language");
            c.Headers.Add("Accept-Language", Culture.EnglishCode);
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(url);
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var codeList = response["containeditems"];

            foreach (var code in codeList)
            {
                JToken codevalueToken = code["codevalue"];
                string codevalue = codevalueToken?.ToString();

                if (string.IsNullOrWhiteSpace(codevalue))
                    codevalue = code["label"].ToString();

                if (!CodeValues.ContainsKey(codevalue))
                {
                    CodeValues.Add(codevalue, code["label"].ToString());
                }
            }

            return CodeValues;
        }

        Dictionary<string, string> OrganizationsEnglish = new Dictionary<string, string>();

        public string GetOrganization(string name)
        {
            var orgNameEnglish = "";
            try {

                if (OrganizationsEnglish.ContainsKey(name))
                    return OrganizationsEnglish[name];
            
            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            Log.Info("Get " + System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "/api/organisasjon/navn/" + name + "/en");
            var data = c.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "/api/organisasjon/navn/" + name + "/en");
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);


            var orgToken = response["Name"];
            if (orgToken != null)
                orgNameEnglish = orgToken.ToString();

            OrganizationsEnglish.Add(name, orgNameEnglish);

            }

            catch (Exception ex)
            {
                Log.Error("Get "+ System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "/api/organisasjon/navn/" + name + "/en failed " + ex.Message);
            }

            return orgNameEnglish;
        }

        OfficeOpenXml.ExcelWorksheet workSheet;

        public void Update(HttpPostedFileBase file, string username, string metadatafield, bool deleteData, string metadatafieldEnglish)
        {
            excelPackage = new OfficeOpenXml.ExcelPackage();
            excelPackage.Load(file.InputStream);
            workSheet = excelPackage.Workbook.Worksheets[1];

            if (metadatafield == "AccessConstraints")
                UpdateRestriction(username);
            else if (metadatafield == "KeywordsNationalTheme")
                UpdateTheme(username);
            else if (metadatafield == "KeywordsNationalInitiative")
                UpdateKeywordsNationalInitiative(username, deleteData);
            else if (metadatafield == "KeywordsInspire")
                UpdateKeywordsInspire(username, deleteData);
            else if (metadatafield == "DistributionFormats")
                UpdateDistributionFormats(username, deleteData);
            else if (metadatafield == "ReferenceSystems")
                UpdateReferenceSystems(username, deleteData);
            else if (metadatafield == "License")
                UpdateLicense(username, deleteData);
            else if (metadatafield == "EnglishTexts")
                UpdateEnglishTexts(username, deleteData, metadatafieldEnglish);
            else if (metadatafield == "SpatialScope")
                UpdateSpatialScope(username, deleteData);
            else if (metadatafield == "ContactOwnerPositionName")
                UpdateContactOwnerPositionName(username, deleteData);

            excelPackage.Dispose();

        }

        private void UpdateContactOwnerPositionName(string username, bool deleteData)
        {

            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;
            if (deleteData)
            {
                for (int row = start.Row + 1; row <= end.Row; row++)
                {
                    var uuidDelete = workSheet.Cells[row, 1].Text;
                    RemoveContactOwnerPositionName(uuidDelete, username);
                }
            }
            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var contactOwnerPositionName = workSheet.Cells[row, 2].Text;
                var uuid = workSheet.Cells[row, 1].Text;

                if (!string.IsNullOrWhiteSpace(contactOwnerPositionName) && !string.IsNullOrWhiteSpace(uuid))
                {
                    SaveContactOwnerPositionName(uuid, contactOwnerPositionName, username);
                }
            }
        }

        private void SaveContactOwnerPositionName(string uuid, string contactOwnerPositionNameInfo, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);

                metadata.ContactOwnerPositionName = contactOwnerPositionNameInfo;

                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch update uuid: " + uuid + ", ContactOwnerPositionName: " + contactOwnerPositionNameInfo);

            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        private void RemoveContactOwnerPositionName(string uuidDelete, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuidDelete);
                if (!string.IsNullOrEmpty(metadata.ContactOwnerPositionName))
                {
                    metadata.ContactOwnerPositionName = "";
                    _metadataService.SaveMetadataModel(metadata, username);

                    Log.Info("Batch remove ContactOwnerPositionName, uuid: " + uuidDelete);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuidDelete, ex);
            }
        }

        private void UpdateSpatialScope(string username, bool deleteData)
        {
            Dictionary<string, string> spatialScopes = _metadataService.GetSpatialScopes();

            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;
            if (deleteData)
            {
                for (int row = start.Row + 1; row <= end.Row; row++)
                {
                    var uuidDelete = workSheet.Cells[row, 1].Text;
                    RemoveSpatialScope(uuidDelete, username);
                }
            }
            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var spatialScope = workSheet.Cells[row, 2].Text;
                var uuid = workSheet.Cells[row, 1].Text;

                if (!string.IsNullOrWhiteSpace(spatialScope) && !string.IsNullOrWhiteSpace(uuid))
                {
                    if (spatialScopes.ContainsValue(spatialScope))
                    {
                        var spatialScopeInfo = spatialScopes.Where(s => s.Value == spatialScope).Select(s => s.Key).First();
                        SaveSpatialScope(uuid, spatialScopeInfo, username);
                    }
                }
            }
        }

        private void SaveSpatialScope(string uuid, string spatialScopeInfo, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
                if (metadata.KeywordsSpatialScope != null && metadata.KeywordsSpatialScope.Count > 0)
                {
                    metadata.KeywordsSpatialScope.RemoveAt(0);
                }
                
                metadata.KeywordsSpatialScope.Add(spatialScopeInfo);

                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch update uuid: " + uuid + ", spatial scope: " + spatialScopeInfo);

            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        private void RemoveSpatialScope(string uuidDelete, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuidDelete);
                if(metadata.KeywordsSpatialScope != null && metadata.KeywordsSpatialScope.Count > 0) { 
                    metadata.KeywordsSpatialScope.RemoveAt(0);
                    _metadataService.SaveMetadataModel(metadata, username);

                    Log.Info("Batch remove spatial scope, uuid: " + uuidDelete);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuidDelete, ex);
            }
        }

        private void UpdateLicense(string username, bool deleteData)
        {
            Dictionary<string, string> listOfAllowedLicenses = GetCodeList("b7a92d72-7ab4-4c2c-8a01-516a0a00344a");

            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;
            if (deleteData)
            {
                for (int row = start.Row + 1; row <= end.Row; row++)
                {
                    var uuidDelete = workSheet.Cells[row, 1].Text;
                    RemoveLicense(uuidDelete, username);
                }
            }
            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var licenseName = workSheet.Cells[row, 2].Text;
                var licenseUrl = workSheet.Cells[row, 3].Text;
                var uuid = workSheet.Cells[row, 1].Text;

                if (!string.IsNullOrWhiteSpace(licenseName) && !string.IsNullOrWhiteSpace(uuid))
                {
                    if (listOfAllowedLicenses.ContainsKey(licenseUrl) && listOfAllowedLicenses.ContainsValue(licenseName))
                        SaveLicense(uuid, licenseName, licenseUrl, username);
                }
            }
        }

        private void SaveLicense(string uuid, string licenseName, string licenseUrl, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);

                metadata.UseConstraints = "license";
                metadata.OtherConstraintsLink = licenseUrl;
                metadata.OtherConstraintsLinkText = licenseName;

                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch update uuid: " + uuid + ", OtherConstraintsLink: " + licenseUrl + ", OtherConstraintsLinkText: " + licenseName);

            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        private void RemoveLicense(string uuidDelete, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuidDelete);
                metadata.UseConstraints = "";
                metadata.OtherConstraintsLink = "";
                metadata.OtherConstraintsLinkText = "";
                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch remove License, uuid: " + uuidDelete);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuidDelete, ex);
            }
        }

        private void UpdateEnglishTexts(string username, bool deleteData, string metadatafieldEnglish)
        {
            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;

            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var english = workSheet.Cells[row, 2].Text;
                var uuid = workSheet.Cells[row, 1].Text;

                if (!string.IsNullOrWhiteSpace(english) && !string.IsNullOrWhiteSpace(uuid))
                {
                        SaveEnglish(uuid, english, username, metadatafieldEnglish);
                }
            }
        }

        private void SaveEnglish(string uuid, string english, string username, string metadatafieldEnglish)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);

                if (metadatafieldEnglish == "EnglishTitle")
                    metadata.EnglishTitle = english;
                else if (metadatafieldEnglish == "EnglishAbstract")
                    metadata.EnglishAbstract = english;
                else if (metadatafieldEnglish == "EnglishSupplementalDescription")
                    metadata.EnglishSupplementalDescription = english;
                else if (metadatafieldEnglish == "EnglishSpecificUsage")
                    metadata.EnglishSpecificUsage = english;
                else if (metadatafieldEnglish == "EnglishPurpose")
                    metadata.EnglishPurpose = english;
                else if (metadatafieldEnglish == "EnglishProcessHistory")
                    metadata.EnglishProcessHistory = english;

                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch update uuid: " + uuid + ", " + metadatafieldEnglish + " = "  + english);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        void UpdateTheme(string username)
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

        void UpdateRestriction(string username)
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


        void UpdateKeywordsNationalInitiative(string username, bool deleteData)
        {
            Dictionary<string, string> listOfAllowedKeywordsNationalInitiative = GetCodeList("37204b11-4802-44b6-80a1-519968bd072f");

            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;
            if (deleteData)
            { 
                for (int row = start.Row + 1; row <= end.Row; row++)
                {
                    var uuidDelete = workSheet.Cells[row, 1].Text;
                    RemoveKeywordNationalInitiative(uuidDelete, username);
                }
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

        void UpdateKeywordsInspire(string username, bool deleteData)
        {
            Dictionary<string, string> listOfAllowedKeywordsInspire = GetCodeList("e7e48bc6-47c6-4e37-be12-08fb9b2fede6");

            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;
            if (deleteData)
            { 
                for (int row = start.Row + 1; row <= end.Row; row++)
                {
                    var uuidDelete = workSheet.Cells[row, 1].Text;
                    RemoveKeywordInspire(uuidDelete, username);
                }
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

        void UpdateDistributionFormats(string username, bool deleteData)
        {
            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;

            if (deleteData)
            { 
                for (int row = start.Row + 1; row <= end.Row; row++)
                {
                    var uuidDelete = workSheet.Cells[row, 1].Text;
                    RemoveDistributionformats(uuidDelete, username);
                }
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

        void UpdateReferenceSystems(string username, bool deleteData)
        {
            Dictionary<string, string> listOfAllowedReferenceSystems = GetListOfReferenceSystems();

            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;
            if (deleteData)
            { 
                for (int row = start.Row + 1; row <= end.Row; row++)
                {
                    var uuidDelete = workSheet.Cells[row, 1].Text;
                    RemoveReferencesystems(uuidDelete, username);
                }
            }
            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var coordinatesystem = workSheet.Cells[row, 2].Text;
                var uuid = workSheet.Cells[row, 1].Text;

                if (!string.IsNullOrWhiteSpace(coordinatesystem) && !string.IsNullOrWhiteSpace(uuid))
                {
                    if (listOfAllowedReferenceSystems.ContainsKey(coordinatesystem))
                        SaveReferenceSystems(uuid, coordinatesystem, username);
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

                metadata.KeywordsNationalInitiative.Remove(keyword);
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
                    metadata.QualitySpecificationTitleInspire = "COMMISSION REGULATION (EU) No 1089/2010 of 23 November 2010 implementing Directive 2007/2/EC of the European Parliament and of the Council as regards interoperability of spatial data sets and services";

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
                metadata.KeywordsInspire.Remove(keyword);
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
                if(metadata.DistributionsFormats.Count == 1 && string.IsNullOrEmpty(metadata.DistributionsFormats[0].Name))
                {
                    metadata.DistributionsFormats[0].FormatName = format;
                    metadata.DistributionsFormats[0].FormatVersion = version;
                }
                else
                {
                    var dsFormat = new SimpleDistribution { FormatName = format, FormatVersion = version };
                    var exists = metadata.DistributionsFormats.Where(f => f.FormatName == format && f.FormatVersion == version).ToList().Count();
                    if (exists == 0)
                        metadata.DistributionsFormats.Add(dsFormat);
                }

                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch update uuid: " + uuid + ", distributionformat: " + format);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        void SaveReferenceSystems(string uuid, string referencesystem, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
                if (metadata.ReferenceSystems == null)
                    metadata.ReferenceSystems = new List<SimpleReferenceSystem>();
                var refSys = new SimpleReferenceSystem { CoordinateSystem = GeoNetworkUtil.GetCoordinatesystemText(referencesystem), CoordinateSystemLink = referencesystem };
                var exists = metadata.ReferenceSystems.Where(r => r.CoordinateSystem == referencesystem || r.CoordinateSystemLink == referencesystem).ToList().Count();
                    if(exists == 0)
                    metadata.ReferenceSystems.Add(refSys);

                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch update uuid: " + uuid + ", referencesystem: " + referencesystem);
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
                metadata.QualitySpecificationTitleInspire = null;
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
                metadata.DistributionsFormats = new List<SimpleDistribution>();
                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch remove Distributionformats, uuid: " + uuid);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        void RemoveReferencesystems(string uuid, string username)
        {
            try
            {
                MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
                metadata.ReferenceSystems = new List<SimpleReferenceSystem>();
                _metadataService.SaveMetadataModel(metadata, username);

                Log.Info("Batch remove Referencesystems, uuid: " + uuid);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting metadata uuid=" + uuid, ex);
            }
        }

        public Dictionary<string, string> GetListOfReferenceSystems()
        {
            Dictionary<string, string> ReferenceSystems = new Dictionary<string, string>();

            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/register/epsg-koder");
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var refs = response["containeditems"];

            foreach (var refsys in refs)
            {

                var documentreference = refsys["documentreference"].ToString();
                if (!ReferenceSystems.ContainsKey(documentreference))
                {
                    ReferenceSystems.Add(documentreference, refsys["label"].ToString());
                }
            }

            ReferenceSystems = ReferenceSystems.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

            return ReferenceSystems;
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

        public void UpdateFormatOrganization(string username)
        {
            string uuid = "";

            try
            {
                //Disable SSL sertificate errors
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                        System.Security.Cryptography.X509Certificates.X509Chain chain,
                                        System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true; // **** Always accept
                };

                System.Net.WebClient c = new System.Net.WebClient();
                c.Encoding = System.Text.Encoding.UTF8;

                string protocol = "https:";
                string kartkatalogenUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["KartkatalogUrl"];
                if (!kartkatalogenUrl.StartsWith("http"))
                    kartkatalogenUrl = protocol + kartkatalogenUrl;


                var data = c.DownloadString(kartkatalogenUrl + "api/search?limit=3000&facets[0]name=type&facets[0]value=dataset");
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);
                var result = response.SelectToken("Results").ToList();


                foreach (var dataset in result.ToList())
                {
                    try
                    {
                        uuid = dataset["Uuid"].ToString();

                        MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);

                        if (metadata.DistributionsFormats != null && metadata.DistributionsFormats.Count > 0 
                            && metadata.ContactPublisher != null && !string.IsNullOrEmpty(metadata.ContactPublisher.Organization))
                        {
                            bool updated = false;
                            for (int f=0; f < metadata.DistributionsFormats.Count; f++)
                            {
                                if (string.IsNullOrEmpty(metadata.DistributionsFormats[f].Organization))
                                { 
                                    metadata.DistributionsFormats[f].Organization = metadata.ContactPublisher.Organization;
                                    updated = true;
                                }
                            }

                            if(updated)
                                _metadataService.SaveMetadataModel(metadata, username);

                        }
                    }
                    catch (Exception e)
                    {
                        Log.Info("Batch UpdateFormatOrganization error, uuid: " + uuid, e);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Batch UpdateFormatOrganization error", ex);
            }

        }

        public void UpdateKeywordServiceType(string username)
        {
            string uuid = "";

            try
            {
                //Disable SSL sertificate errors
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                        System.Security.Cryptography.X509Certificates.X509Chain chain,
                                        System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true; // **** Always accept
                };

                System.Net.WebClient c = new System.Net.WebClient();
                c.Encoding = System.Text.Encoding.UTF8;

                string protocol = "https:";
                string kartkatalogenUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["KartkatalogUrl"];
                if (!kartkatalogenUrl.StartsWith("http"))
                    kartkatalogenUrl = protocol + kartkatalogenUrl;


                var data = c.DownloadString(kartkatalogenUrl + "api/search?limit=2000&facets[0]name=type&facets[0]value=service");
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);
                var result1 = response.SelectToken("Results").ToList();

                var data2 = c.DownloadString(kartkatalogenUrl + "api/search?limit=2000&facets[0]name=type&facets[0]value=servicelayer");
                var response2 = Newtonsoft.Json.Linq.JObject.Parse(data2);
                var result2 = response2.SelectToken("Results").ToList();

                var result = result1.Concat(result2);


                foreach (var dataset in result.ToList())
                {
                    try
                    {
                        uuid = dataset["Uuid"].ToString();

                        MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);

                        if (metadata.DistributionsFormats != null && metadata.DistributionsFormats.Count > 0)
                        {
                            bool updatedNeeded = false;
                            if (!string.IsNullOrEmpty(metadata.DistributionsFormats[0].Protocol))
                                updatedNeeded = true;

                            if (updatedNeeded)
                                _metadataService.SaveMetadataModel(metadata, username);

                        }
                    }
                    catch (Exception e)
                    {
                        Log.Info("Batch UpdateKeywordServiceType error, uuid: " + uuid, e);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Batch UpdateKeywordServiceType error", ex);
            }
        }


        internal void GenerateMediumThumbnails(string username, string organization, string folder)
        {
            this.thumbnailFolder = folder;
            try
            {
                BatchData data = new BatchData();
                MetadataIndexViewModel model = new MetadataIndexViewModel();
                int offset = 1;
                int limit = 50;
                model = _metadataService.SearchMetadata("", "", offset, limit);
                model.UserOrganization = organization;

                foreach (var item in model.MetadataItems)
                {
                    data.MetaData.Add(new MetaDataEntry { Uuid = item.Uuid });
                }

                CheckThumbnails(data, username);

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

                    CheckThumbnails(data, username);

                    next = model.OffsetNext();
                    if (next == 0) break;
                }


            }

            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

        }

        private void CheckThumbnails(BatchData data, string username)
        {
            try
            {
                foreach (var md in data.MetaData)
                {
                    try
                    {
                        bool metadataUpdated = false;

                        MetadataViewModel metadata = _metadataService.GetMetadataModel(md.Uuid);

                        if (metadata.Thumbnails != null)
                        {
                            var largeThumbnail = metadata.Thumbnails.Where(t => t.Type == "large_thumbnail").FirstOrDefault();
                            var mediumThumbnail = metadata.Thumbnails.Where(t => t.Type == "medium").FirstOrDefault();

                            if (largeThumbnail != null && largeThumbnail.URL.Contains("geonorge.no/thumbnails") && mediumThumbnail == null)
                            {
                                var urlLarge = largeThumbnail.URL;

                                string filenamePathLarge;
                                string filename;
                                System.Uri uri = new System.Uri(urlLarge);
                                filename = System.IO.Path.GetFileName(uri.LocalPath);
                                filenamePathLarge = thumbnailFolder + filename;


                                if (!string.IsNullOrEmpty(filenamePathLarge))
                                {
                                    var filenameMedium = filename.Replace(".", "_medium.");
                                    var url = System.Web.Configuration.WebConfigurationManager.AppSettings["EditorUrl"] + "thumbnails/" + filenameMedium;
                                    var fullPathMedium = thumbnailFolder + filenameMedium;

                                    if (File.Exists(filenamePathLarge))
                                    {
                                        var imageInfo = ImageResizer.ImageBuilder.Current.LoadImageInfo(filenamePathLarge, null);
                                        var widthSource = imageInfo["source.width"]?.ToString();
                                        int width = 0;
                                        Int32.TryParse(widthSource, out width);
                                        int maxWidth = 300;

                                        if (width > maxWidth)
                                        {
                                            OptimizeImage(filenamePathLarge, maxWidth, 1000, fullPathMedium);

                                            metadata.Thumbnails.Add(new Thumbnail { Type = "medium", URL = url });
                                            metadataUpdated = true;
                                        }
                                        else
                                        {
                                            Log.Info("Metadata with uuid: " + md.Uuid + " has a large image width width: " + width.ToString());
                                        }
                                    }
                                }

                            }

                            if (metadataUpdated)
                            {
                                _metadataService.SaveMetadataModel(metadata, username);
                                Log.Info("Batch update medium thumbnail uuid: " + metadata.Uuid);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Metadata with uuid:" + md.Uuid + " failed to generate thumbnail with error: " + ex.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

        }

        public static void OptimizeImage(string file, int maxWidth, int maxHeight, string outputPath, int quality = 70)
        {
            ImageResizer.ImageJob newImage =
                new ImageResizer.ImageJob(file, outputPath,
                new ImageResizer.Instructions("maxwidth=" + maxWidth + ";maxheight=" + maxHeight + ";quality=" + quality));

            newImage.Build();
   
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

        void IBatchService.GenerateMediumThumbnails(string v1, string v2, string v3)
        {
            throw new NotImplementedException();
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