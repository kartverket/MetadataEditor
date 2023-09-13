using Kartverket.MetadataEditor.Models;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Kartverket.MetadataEditor.Models.OpenData;
using Kartverket.MetadataEditor.Models.Mets;

namespace Kartverket.MetadataEditor.Controllers
{
    public class BatchController : ControllerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IBatchService _batchService;
        private readonly IMetadataService _metadataService;
        private readonly IOpenMetadataService _openMetadataService;
        private readonly IMetsMetadataService _metsMetadataService;
        private readonly MetadataContext _db;

        public BatchController(IMetadataService metadataService, IBatchService batchService, IOpenMetadataService openMetadataService, MetadataContext dbContext, IMetsMetadataService metsMetadataService)
        {
            _metadataService = metadataService;
            _batchService = batchService;
            _openMetadataService = openMetadataService;
            _db = dbContext;
            _metsMetadataService = metsMetadataService;
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
                        new Thread(() => _batchService.UpdateAll(data, GetUsername(), GetCurrentUserOrganizationName())).Start();
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
                        new Thread(() => _batchService.Update(data, GetUsername())).Start();
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
            if (!UserHasMetadataAdminRole())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            Log.Info("Starting batch update thumbnail generation.");
            new Thread(() => _batchService.GenerateMediumThumbnails(GetUsername(), GetCurrentUserOrganizationName(), Server.MapPath("~/thumbnails/"))).Start();
            TempData["message"] = "Batch-oppdatering: generering av thumbnails er startet og kjører i bakgrunnen!";
               
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
                    _batchService.Update(file, GetUsername(), metadatafield, deleteData, metadatafieldEnglish);

                    TempData["Message"] = "Metadataene ble oppdatert";
                }
                else
                {
                    TempData["failure"] = "Vennligst velg .xlsx filtype";
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult OpenData()
        {
            if (!UserHasMetadataAdminRole())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            string username = GetUsername();

            var endpoints = _db.OpenMetadataEndpoints.ToList();
            new Thread(() => _openMetadataService.SynchronizeMetadata(endpoints, username)).Start();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult MetsData()
        {
            if (!UserHasMetadataAdminRole())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            string username = GetUsername();

            new Thread(() => _metsMetadataService.SynchronizeMetadata(username)).Start();
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult UpdateFormatOrganization()
        {
            new Thread(() => _batchService.UpdateFormatOrganization(GetUsername())).Start();
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
        }

        [Authorize]
        public ActionResult SyncronizeRegisterTranslations(string uuid = null)
        {
            if (!UserHasMetadataAdminRole())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            new Thread(() => _batchService.UpdateRegisterTranslations(GetUsername(), uuid)).Start();
            TempData["message"] = "Batch-oppdatering: synkronisering av engelske register tekster kjører i bakgrunnen!";
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult SyncronizeAdminUnitsUri()
        {
            if (!UserHasMetadataAdminRole())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            
            new Thread(() => _batchService.UpdateKeywordPlaceUri(GetUsername())).Start();
            TempData["message"] = "Batch-oppdatering av stedsnavn URI for geografiske nøkkelord kjører i bakgrunnen!";
            return RedirectToAction("Index");
        }


        [Authorize]
        public ActionResult UpdateKeywordServiceType()
        {
            new Thread(() => _batchService.UpdateKeywordServiceType(GetUsername())).Start();
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
                string userOrganization = GetCurrentUserOrganizationName();
                if (UserHasMetadataAdminRole())
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