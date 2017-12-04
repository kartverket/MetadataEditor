using Kartverket.MetadataEditor.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Controllers
{
    public class BatchController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        private MetadataService _metadataService;

        public BatchController()
        {
            _metadataService = new MetadataService();
        }

        [Authorize]
        public ActionResult BatchUpdate(FormCollection batch) 
        {
            if (User.Identity.IsAuthenticated)
            {
                Log.Info("Starting batch update metadata.");

                BatchData data = GetFormData(batch);

                if (!string.IsNullOrWhiteSpace(Request.Form["updateAll"])) {

                    if (data != null)
                    {
                        new Thread(() => new BatchService().UpdateAll(data, GetUsername(), GetSecurityClaim("organization"))).Start();
                        TempData["message"] = "Batch-oppdatering: " + data.dataField  +" = "  + data.dataValue + ", er startet og kjører i bakgrunnen!";
                    }
                    else
                    {
                        TempData["failure"] = "Ingen oppdatering valgt";
                    }
            
                }
                else 
                { 
                    if (data != null)
                    {
                        new Thread(() => new BatchService().Update(data, GetUsername())).Start();
                        TempData["message"] = "Batch-oppdatering: " + data.dataField + " = " + data.dataValue + ", er startet og kjører i bakgrunnen!";
                    }
                    else 
                    {
                        TempData["failure"] = "Ingen oppdatering valgt";
                    }
                }

            }

                return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult ThumbnailGeneration()
        {
            if (User.Identity.IsAuthenticated)
            {
                string userOrganization = GetSecurityClaim("organization");
                string role = GetSecurityClaim("role");
                if (!string.IsNullOrWhiteSpace(role) && role.Equals("nd.metadata_admin"))
                { 
                    Log.Info("Starting batch update thumbnail generation.");
                    new Thread(() => new BatchService().GenerateMediumThumbnails(GetUsername(), GetSecurityClaim("organization"), Server.MapPath("~/thumbnails/"))).Start();
                    TempData["message"] = "Batch-oppdatering: generering av thumbnails er startet og kjører i bakgrunnen!";
                }
                else
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            }

            return RedirectToAction("Index");
        }


        [Authorize]
        [OutputCache(Duration = 0)]
        public ActionResult UploadFile(string metadatafield, bool deleteData = false, string metadatafieldEnglish = "")
        {
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];

                if (file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    BatchService batchService = new BatchService();
                    batchService.Update(file, GetUsername(), metadatafield, deleteData, metadatafieldEnglish);

                    TempData["Message"] = "Metadataene ble oppdatert";
                }
                else
                {
                    TempData["failure"] = "Vennligst velg .xlsx filtype";
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult UpdateFormatOrganization()
        {
            new Thread(() => new BatchService().UpdateFormatOrganization(GetUsername())).Start();
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
        }

        [Authorize]
        public ActionResult SyncronizeRegisterTranslations()
        {
            string role = GetSecurityClaim("role");
            if (!string.IsNullOrWhiteSpace(role) && role.Equals("nd.metadata_admin"))
            {
                new Thread(() => new BatchService().UpdateRegisterTranslations(GetUsername())).Start();
                TempData["message"] = "Batch-oppdatering: synkronisering av engelske register tekster kjører i bakgrunnen!";
                return RedirectToAction("Index");
            }
            else
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

        }

        [Authorize]
        public ActionResult UpdateKeywordServiceType()
        {
            new Thread(() => new BatchService().UpdateKeywordServiceType(GetUsername())).Start();
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
        }

        private BatchData GetFormData(FormCollection batch)
        {

            BatchData data = new BatchData();

            data.dataField = batch["batchField"];
            string dataValue = batch["batchValue"];
            if (data.dataField == "ContactMetadata_Organization" || data.dataField == "ContactPublisher_Organization" || data.dataField == "ContactOwner_Organization")
            {
                dataValue = batch["OrganizationContactMetadata"];
            }
            else if (data.dataField == "MaintenanceFrequency")
            {
                dataValue = batch["MaintenanceFrequency"];
            }

            data.dataValue = dataValue;

            if (string.IsNullOrWhiteSpace(data.dataField) || string.IsNullOrWhiteSpace(data.dataValue))
                return null;

            List<string> uuids = new List<string>();
            if (batch["uuids"] != null) 
            { 
            uuids = batch["uuids"].Split(',').ToList();
            }

            if (string.IsNullOrWhiteSpace(Request.Form["updateAll"]) && uuids.Count == 0)
                return null;

            List<MetaDataEntry> mdList = new List<MetaDataEntry>();

            foreach (var uuid in uuids)
            {
                MetaDataEntry md = new MetaDataEntry();
                md.Uuid = uuid;
                mdList.Add(md);
            }

            data.MetaData = mdList;

            return data;
        }

        public ActionResult Index(MetadataMessages? message, string organization = "", string searchString = "", int offset = 1, int limit = 100)
        {
            ViewBag.StatusMessage =
                message == MetadataMessages.InvalidUuid ? Resources.UI.Error_InvalidUuid
                : "";

            MetadataIndexViewModel model = new MetadataIndexViewModel();

            if (User.Identity.IsAuthenticated)
            {
                string userOrganization = GetSecurityClaim("organization");
                string role = GetSecurityClaim("role");
                if (!string.IsNullOrWhiteSpace(role) && role.Equals("nd.metadata_admin"))
                {
                    model = _metadataService.SearchMetadata(organization, searchString, offset, limit);
                    model.UserIsAdmin = true;
                }
                else
                {
                    model = _metadataService.SearchMetadata(userOrganization, searchString, offset, limit);
                }

                model.UserOrganization = userOrganization;
            }

            Dictionary<string, string> OrganizationList = GetListOfOrganizations();
            ViewBag.OrganizationContactMetadata = new SelectList(OrganizationList, "Key", "Value");
            ViewBag.MaintenanceFrequency = new SelectList(GetListOfMaintenanceFrequencyValues(), "Key", "Value");


            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View(model);
        }

        private string GetUsername()
        {
            return GetSecurityClaim("username");
        }

        private string GetSecurityClaim(string type)
        {
            string result = null;
            foreach (var claim in System.Security.Claims.ClaimsPrincipal.Current.Claims)
            {
                if (claim.Type == type && !string.IsNullOrWhiteSpace(claim.Value))
                {
                    result = claim.Value;
                    break;
                }
            }

            // bad hack, must fix BAAT
            if (!string.IsNullOrWhiteSpace(result) && type.Equals("organization") && result.Equals("Statens kartverk"))
            {
                result = "Kartverket";
            }

            return result;
        }

        public Dictionary<string, string> GetListOfOrganizations()
        {
            Dictionary<string, string> Organizations = new Dictionary<string, string>();

            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/register/organisasjoner");
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var orgs = response["containeditems"];

            foreach (var org in orgs)
            {
                if (!Organizations.ContainsKey(org["label"].ToString()))
                {
                    Organizations.Add(org["label"].ToString(), org["label"].ToString());
                }
            }

            Organizations = Organizations.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

            return Organizations;
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

        public Dictionary<string, string> GetListOfMaintenanceFrequencyValues()
        {

            return GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB124");
        }

    }
}