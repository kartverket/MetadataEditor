using System.Configuration;
using System.IO;
using Kartverket.MetadataEditor.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kartverket.MetadataEditor.Util;
using Resources;
using log4net;
using Newtonsoft.Json.Linq;

namespace Kartverket.MetadataEditor.Controllers
{
    [HandleError]
    public class SimpleMetadataController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        private SimpleMetadataService _metadataService;

        public SimpleMetadataController()
        {
            _metadataService = new SimpleMetadataService();
        }

        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            MetadataCreateViewModel model = new MetadataCreateViewModel
            {
                MetadataContactName = GetSecurityClaim("urn:oid:1.3.6.1.4.1.5923.1.1.1.6"),
                MetadataContactOrganization = GetSecurityClaim("organization"),
                MetadataContactEmail = GetSecurityClaim("urn:oid:1.2.840.113549.1.9.1"),
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(MetadataCreateViewModel model)
        {
            string organization = GetSecurityClaim("organization");
            model.MetadataContactOrganization = organization;
            if (ModelState.IsValid)
            {
                string username = GetUsername();
                string uuid = _metadataService.CreateMetadata(model, username);
                Log.Info(string.Format("Created new metadata: {0} [uuid = {1}] for user: {2} on behalf of {3} ", model.Title, uuid, username, organization));
                return RedirectToAction("Edit", new { uuid = uuid, metadatacreated = true });
            }
            return View(model);
        }

        public ActionResult Index(MetadataMessages? message, string organization = "", string searchString = "", int offset = 1, int limit = 50)
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


        [HttpGet]
        [Authorize]
        public ActionResult Edit(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return HttpNotFound();

            try
            {
                SimpleMetadataViewModel model = _metadataService.GetMetadataModel(uuid);
                string role = GetSecurityClaim("role");
                if (HasAccessToMetadata(model))
                {
                    PrepareViewBagForEditing(model);
                    return View(model);
                }
                else
                {
                    TempData["failure"] = "Du har ikke tilgang til å redigere disse metadataene";
                    return View("Error");
                }
            }
            catch (Exception e)
            {
                //Log.Error("Error while getting metadata with uuid = " + uuid, e);
                throw new Exception("Error while getting metadata with uuid = " + uuid, e);
                //TempData["failure"] = "Feilmelding: " + e.Message;
                //return View("Error");
            }


        }


        private void PrepareViewBagForEditing(SimpleMetadataViewModel model)
        {
           
            ViewBag.MaintenanceFrequencyValues = new SelectList(GetListOfMaintenanceFrequencyValues(), "Key", "Value", model.MaintenanceFrequency);
           
            ViewBag.CreateProductSheetUrl =
                System.Web.Configuration.WebConfigurationManager.AppSettings["ProductSheetGeneratorUrl"] + model.Uuid;
            ViewBag.DatasetUrl =
                System.Web.Configuration.WebConfigurationManager.AppSettings["DownloadDatasetUrl"];
            ViewBag.GeoNetworkViewUrl = GeoNetworkUtil.GetViewUrl(model.Uuid);
            ViewBag.GeoNetworkXmlDownloadUrl = GeoNetworkUtil.GetXmlDownloadUrl(model.Uuid);
            var seoUrl = new SeoUrl("", model.Title);
            ViewBag.KartkatalogViewUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["KartkatalogUrl"] + "Metadata/" + seoUrl.Title + "/" + model.Uuid;
            ViewBag.predefinedDistributionProtocols = new SelectList(GetListOfpredefinedDistributionProtocols(), "Key", "Value");

            Dictionary<string, string> OrganizationList = GetListOfOrganizations();


            if (string.IsNullOrEmpty(model.ContactMetadata.Organization))
            {
                if (Request.Form["ContactMetadata.Organization.Old"] != null || !string.IsNullOrWhiteSpace(Request.Form["ContactMetadata.Organization.Old"]))
                {
                    model.ContactMetadata.Organization = Request.Form["ContactMetadata.Organization.Old"].ToString();
                }
            }

            if (string.IsNullOrEmpty(model.ContactPublisher.Organization))
            {
                if (Request.Form["ContactPublisher.Organization.Old"] != null || !string.IsNullOrWhiteSpace(Request.Form["ContactPublisher.Organization.Old"]))
                {
                    model.ContactPublisher.Organization = Request["ContactPublisher.Organization.Old"].ToString();
                }
            }


            if (string.IsNullOrEmpty(model.ContactOwner.Organization))
            {
                if (Request.Form["ContactOwner.Organization.Old"] != null || !string.IsNullOrWhiteSpace(Request.Form["ContactOwner.Organization.Old"]))
                {
                    model.ContactOwner.Organization = Request["ContactOwner.Organization.Old"].ToString();
                }
            }

            ViewBag.OrganizationContactMetadataValues = new SelectList(OrganizationList, "Key", "Value", model.ContactMetadata.Organization);
            ViewBag.OrganizationContactPublisherValues = new SelectList(OrganizationList, "Key", "Value", model.ContactPublisher.Organization);
            ViewBag.OrganizationContactOwnerValues = new SelectList(OrganizationList, "Key", "Value", model.ContactOwner.Organization);

            ViewBag.NationalThemeValues = new SelectList(GetListOfNationalTheme(), "Key", "Value");
            ViewBag.NationalInitiativeValues = new SelectList(GetListOfNationalInitiative(), "Key", "Value");
            ViewBag.InspireValues = new SelectList(GetListOfInspire(), "Key", "Value");

            ViewBag.UseConstraintValues = new SelectList(GetListOfRestrictionValues(), "Key", "Value", model.UseConstraints);
            ViewBag.LicenseTypesValues = new SelectList(GetListOfLicenseTypes(), "Key", "Value", model.OtherConstraintsLink);


            ViewBag.ValideringUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["ValideringUrl"] + "api/metadata/" + model.Uuid;

            ViewBag.Municipalities = new KomDataService().GetListOfMunicipalityOrganizations();

        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(string uuid, string action, SimpleMetadataViewModel model, string ignoreValidationError)
        {
            ViewBag.IsAdmin = "0";
            string role = GetSecurityClaim("role");
            if (!string.IsNullOrWhiteSpace(role) && role.Equals("nd.metadata_admin"))
            {
                ViewBag.IsAdmin = "1";
            }


            if (ModelState.IsValid)
            {
               
                    SaveMetadataToCswServer(model);
                    if (action.Equals(UI.Button_Validate))
                    {
                        ValidateMetadata(uuid);
                    }

                    return RedirectToAction("Edit", new { uuid = model.Uuid });
               
            }
            else
            {
                string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                Log.Debug("Model for " + uuid + " is not valid: " + messages);
            }

            PrepareViewBagForEditing(model);
            return View(model);
        }


        private void SaveMetadataToCswServer(SimpleMetadataViewModel model)
        {
            try
            {
                _metadataService.SaveMetadataModel(model, GetUsername());
                TempData["success"] = UI.Metadata_Edit_Saved_Success;
            }
            catch (Exception e)
            {
                Log.Error("Error while editing metadata with uuid = " + model.Uuid, e);
                TempData["failure"] = String.Format(UI.Metadata_Edit_Saved_Failure, e.Message);
            }
        }


        private void ValidateMetadata(string uuid)
        {
            try
            {
                if (System.Web.Configuration.WebConfigurationManager.AppSettings["ValideringUrl"] != "")
                {
                    System.Net.WebClient c = new System.Net.WebClient();
                    c.Encoding = System.Text.Encoding.UTF8;
                    var data = c.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["ValideringUrl"] + "api/validate/" + uuid);
                    TempData["success"] = UI.Metadata_Validate_Success;
                }
            }
            catch (Exception e)
            {
                TempData["failure"] = String.Format(UI.Metadata_Validate_Error, e.Message);
            }

        }

        public Dictionary<string, string> GetListOfTopicCategories()
        {
            return GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB100");
        }

        public Dictionary<string, string> GetListOfUnitsOfDistribution()
        {

            return GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB119");
        }

        public Dictionary<string, string> GetListOfSpatialRepresentations()
        {

            return GetCodeList("4C54EB31-714E-4457-AF6A-44FE6DBE76C1");
        }

        public Dictionary<string, string> GetListOfMaintenanceFrequencyValues()
        {

            return GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB124");
        }

        public Dictionary<string, string> GetListOfStatusValues()
        {

            return GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB137");
        }

        public Dictionary<string, string> GetListOfClassificationValues()
        {

            return GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB145");
        }

        public Dictionary<string, string> GetListOfRestrictionValues()
        {

            return GetCodeList("D23E9F2F-66AB-427D-8AE4-5B6FD3556B57");

        }

        public Dictionary<string, string> GetListOfpredefinedDistributionProtocols()
        {
            Dictionary<string, string> dic = GetCodeList("94B5A165-7176-4F43-B6EC-1063F7ADE9EA");

            // Get only simplemetadata dataset related values
            var keysToSelect = new List<string>
            {
               {"GEONORGE:FILEDOWNLOAD"},
               {"GEONORGE:DOWNLOAD"},
               {"GEONORGE:OFFLINE"},
               {"WWW:DOWNLOAD-1.0-http--download"},
               {"W3C:REST"},
               {"W3C:WS"},
               {"WWW:LINK-1.0-http--link"},
            };

            Dictionary<string, string> newDic = new Dictionary<string, string>();
            foreach (var key in keysToSelect)
            {
                if (dic.ContainsKey(key))
                {
                    var elem = new KeyValuePair<string, string>(key, dic[key]);
                    newDic.Add(elem.Key, elem.Value);
                }
                   
            }
            return newDic;
        }


        public Dictionary<string, string> GetListOfNationalTheme()
        {
            return GetCodeList("42CECF70-0359-49E6-B8FF-0D6D52EBC73F");
        }

        public Dictionary<string, string> GetListOfNationalInitiative()
        {
            return GetCodeList("37204B11-4802-44B6-80A1-519968BD072F");
        }

        public Dictionary<string, string> GetListOfInspire()
        {
            return GetCodeList("E7E48BC6-47C6-4E37-BE12-08FB9B2FEDE6");
        }

        public Dictionary<string, string> GetListOfVectorFormats()
        {
            return GetCodeList("49202645-7137-499F-8CA3-F0F89324B107");
        }

        public Dictionary<string, string> GetListOfRasterFormats()
        {
            return GetCodeList("25EF67D3-974F-4B0C-841D-BDD0B29CE78B");
        }


        public Dictionary<string, string> GetCodeList(string systemid)
        {
            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue(systemid);

            Dictionary<string, string> CodeValues = new Dictionary<string, string>();

            if (cache != null)
            {
                CodeValues = cache as Dictionary<string, string>;
            }
            else
            {
                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/kodelister/" + systemid;
                System.Net.WebClient c = new System.Net.WebClient();
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
            }
            CodeValues = CodeValues.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
            memCacher.Add(systemid, CodeValues, new DateTimeOffset(DateTime.Now.AddYears(1)));

            return CodeValues;
        }


        public Dictionary<string, string> GetListOfOrganizations()
        {
            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue("organizations");

            Dictionary<string, string> Organizations = new Dictionary<string, string>();

            if (cache != null)
            {
                Organizations = cache as Dictionary<string, string>;
            }
            else
            {
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
            }
            Organizations = Organizations.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
            memCacher.Add("organizations", Organizations, new DateTimeOffset(DateTime.Now.AddYears(1)));

            return Organizations;
        }

        public Dictionary<string, string> GetListOfLicenseTypes()
        {
            return GetCodeList("5b2a3f96-f092-41f9-8196-0e1dd5c2c134"); //Todo use uuid from production
        }

        [Authorize]
        [OutputCache(Duration = 0)]
        public ActionResult UploadDataset(string uuid)
        {
            string filename = null;
            var viewresult = Json(new { });
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];

                if (file.ContentType == "application/x-zip-compressed")
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    filename = uuid + ".zip";
                    string fullPath = Server.MapPath("~/datasets/" + filename);
                    
                        file.SaveAs(fullPath);

                    viewresult = Json(new { status = "OK", filename = filename });
                }
                else
                {
                    viewresult = Json(new { status = "ErrorWrongContent" });
                }
            }

            //for IE8 which does not accept application/json
            if (Request.Headers["Accept"] != null && !Request.Headers["Accept"].Contains("application/json"))
                viewresult.ContentType = "text/plain";

            return viewresult;
        }


        [Authorize]
        [HttpGet]
        public ActionResult ConfirmDelete(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return HttpNotFound();

            SimpleMetadataViewModel model = _metadataService.GetMetadataModel(uuid);

            string role = GetSecurityClaim("role");
            if (HasAccessToMetadata(model))
            {
                return View(model);
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Delete(string uuid)
        {
            SimpleMetadataViewModel model = _metadataService.GetMetadataModel(uuid);

            string role = GetSecurityClaim("role");
            if (HasAccessToMetadata(model))
            {
                _metadataService.DeleteMetadata(uuid, GetUsername());

                TempData["Message"] = "Metadata med uuid " + uuid + " ble slettet.";
                return RedirectToAction("Index");
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        private bool HasAccessToMetadata(SimpleMetadataViewModel model)
        {
            string organization = GetSecurityClaim("organization");
            string role = GetSecurityClaim("role");
            bool isAdmin = !string.IsNullOrWhiteSpace(role) && role.Equals("nd.metadata_admin");
            return isAdmin || model.HasAccess(organization);
        }

        [Authorize]
        public ActionResult RegisterData()
        {
            string organization = GetSecurityClaim("organization");
            ViewBag.RegisterOrganizationUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/register/search/organisasjon/" + organization;
            return View();

        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }

    }

}