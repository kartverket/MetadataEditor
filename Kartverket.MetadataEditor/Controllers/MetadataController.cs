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
using System.Net;
using Newtonsoft.Json.Linq;

namespace Kartverket.MetadataEditor.Controllers
{
    [HandleError]
    public class MetadataController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        private MetadataService _metadataService;

        public MetadataController()
        {
            _metadataService = new MetadataService();
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
                MetadataViewModel model = _metadataService.GetMetadataModel(uuid);
                string role = GetSecurityClaim("role");
                if (!string.IsNullOrWhiteSpace(role) && role.Equals("nd.metadata_admin"))
                    model.ValidateAllRequirements = true;

                if (HasAccessToMetadata(model))
                {
                    if (model.MetadataStandard == "ISO19115:Norsk versjon" && Request.QueryString["editor"] == null)
                        return RedirectToAction("Edit", "SimpleMetadata", new { uuid = uuid });

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


        private void PrepareViewBagForEditing(MetadataViewModel model)
        {
            ViewBag.TopicCategoryValues = new SelectList(GetListOfTopicCategories(), "Key", "Value", model.TopicCategory);
            ViewBag.SpatialRepresentationValues = new SelectList(GetListOfSpatialRepresentations(), "Key", "Value", model.SpatialRepresentation);

            ViewBag.VectorFormats = new SelectList(GetListOfVectorFormats(), "Key", "Value");
            ViewBag.RasterFormats = new SelectList(GetListOfRasterFormats(), "Key", "Value");

            ViewBag.predefinedDistributionProtocols = new SelectList(GetListOfpredefinedDistributionProtocols(), "Key", "Value");
            ViewBag.UnitsOfDistributionValues = new SelectList(GetListOfUnitsOfDistribution(), "Key", "Value", model.UnitsOfDistribution);
            ViewBag.MaintenanceFrequencyValues = new SelectList(GetListOfMaintenanceFrequencyValues(), "Key", "Value", model.MaintenanceFrequency);
            ViewBag.StatusValues = new SelectList(GetListOfStatusValues(), "Key", "Value", model.Status);
            ViewBag.SecurityConstraintValues = new SelectList(GetListOfClassificationValues(), "Key", "Value", model.SecurityConstraints);
            ViewBag.UseConstraintValues = new SelectList(GetListOfRestrictionValues(), "Key", "Value", model.UseConstraints);
            ViewBag.LicenseTypesValues = new SelectList(GetListOfLicenseTypes(), "Key", "Value", model.OtherConstraintsLink);
            if (!string.IsNullOrEmpty(model.OtherConstraintsAccess) && (model.OtherConstraintsAccess.ToLower() == "no restrictions" || model.OtherConstraintsAccess.ToLower() == "norway digital restricted"))
            {
                model.AccessConstraints = model.OtherConstraintsAccess;
            }
            ViewBag.AccessConstraintValues = new SelectList(GetListOfRestrictionValuesAdjusted(), "Key", "Value", model.AccessConstraints);
            ViewBag.CreateProductSheetUrl =
                System.Web.Configuration.WebConfigurationManager.AppSettings["ProductSheetGeneratorUrl"] + model.Uuid;
            ViewBag.ThumbnailUrl =
                System.Web.Configuration.WebConfigurationManager.AppSettings["EditorUrl"] + "thumbnails/";
            ViewBag.GeoNetworkViewUrl = GeoNetworkUtil.GetViewUrl(model.Uuid);
            ViewBag.GeoNetworkXmlDownloadUrl = GeoNetworkUtil.GetXmlDownloadUrl(model.Uuid);
            var seoUrl = new SeoUrl("", model.Title);
            ViewBag.KartkatalogViewUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["KartkatalogUrl"] + "Metadata/" + seoUrl.Title + "/" + model.Uuid;

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
                if (Request.Form["ContactOwner.Organization.Old"] != null || !string.IsNullOrWhiteSpace(Request.Form["ContactOwner.Organization.Old"])) {
                    model.ContactOwner.Organization = Request["ContactOwner.Organization.Old"].ToString();
                }
            }

            ViewBag.OrganizationContactMetadataValues = new SelectList(OrganizationList, "Key", "Value", model.ContactMetadata.Organization);
            ViewBag.OrganizationContactPublisherValues = new SelectList(OrganizationList, "Key", "Value", model.ContactPublisher.Organization);
            ViewBag.OrganizationContactOwnerValues = new SelectList(OrganizationList, "Key", "Value", model.ContactOwner.Organization);
            ViewBag.OrganizationDistributorValues = new SelectList(OrganizationList, "Key", "Value");

            Dictionary<string, string> ReferenceSystemsList = GetListOfReferenceSystems();
            ViewBag.ReferenceSystemsValues = new SelectList(ReferenceSystemsList, "Key", "Value");

            ViewBag.NationalThemeValues = new SelectList(GetListOfNationalTheme(), "Key", "Value");
            ViewBag.NationalInitiativeValues = new SelectList(GetListOfNationalInitiative(), "Key", "Value");
            ViewBag.CatalogValues = new SelectList(GetListOfCatalogs(), "Key", "Value");
            ViewBag.InspireValues = new SelectList(GetListOfInspire(), "Key", "Value");

            IEnumerable<SelectListItem> conceptItems = from concept in model.KeywordsConcept
                                                select new SelectListItem
                                                {
                                                    Text = concept.Split('|')[0].ToString(),
                                                    Value = concept.ToString(),
                                                    Selected = true,
                                                };
            ViewBag.Concepts = new MultiSelectList(conceptItems,"Value", "Text", conceptItems.Select(c => c.Value).ToArray());

            var productspesifications = GetRegister("produktspesifikasjoner", model);           
            if(!string.IsNullOrEmpty(model.ProductSpecificationUrl))
            {
                KeyValuePair<string, string> prodspecSelected = new KeyValuePair<string, string>(model.ProductSpecificationUrl, model.ProductSpecificationUrl);
                if (!productspesifications.ContainsKey(prodspecSelected.Key))
                {
                    productspesifications.Add(prodspecSelected.Key, prodspecSelected.Value);
                }
            }
            ViewBag.ProductspesificationValues = new SelectList(productspesifications, "Key", "Value", model.ProductSpecificationUrl);

            var orderingInstructions = GetSubRegister("metadata-kodelister/kartverket/norge-digitalt-tjenesteerklaering", model);
            if (!string.IsNullOrEmpty(model.OrderingInstructions))
            {
                KeyValuePair<string, string> orderingInstructionsSelected = new KeyValuePair<string, string>(model.OrderingInstructions, model.OrderingInstructions);
                if (!orderingInstructions.ContainsKey(orderingInstructionsSelected.Key))
                {
                    orderingInstructions.Add(orderingInstructionsSelected.Key, orderingInstructionsSelected.Value);
                }
            }
            ViewBag.OrderingInstructionsValues = new SelectList(orderingInstructions, "Key", "Value", model.OrderingInstructions);

            ViewBag.ProductsheetValues = new SelectList(GetRegister("produktark", model), "Key", "Value", model.ProductSheetUrl);
            ViewBag.LegendDescriptionValues = new SelectList(GetRegister("tegneregler", model), "Key", "Value", model.LegendDescriptionUrl);

            ViewBag.ValideringUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["ValideringUrl"] + "api/metadata/" + model.Uuid;

            ViewBag.Municipalities = new KomDataService().GetListOfMunicipalityOrganizations();

            ViewBag.ValidModel = true;
            if (Request.QueryString["metadatacreated"] == null )
            {
                ViewBag.ValidModel = TryValidateModel(model);
            }

            ViewBag.NewDistribution = false;

            ViewBag.IsAdmin = "0";
            string role = GetSecurityClaim("role");
            if (!string.IsNullOrWhiteSpace(role) && role.Equals("nd.metadata_admin"))
            {
                ViewBag.IsAdmin = "1";
            }

        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(string uuid, string action, MetadataViewModel model, string ignoreValidationError)
        {
            ValidateModel(model);

            if (ignoreValidationError == "1" /*&& ViewBag.IsAdmin == "1"*/) 
            { 
                foreach (var modelValue in ModelState.Values)
                {
                    modelValue.Errors.Clear();
                }
            }

            model.FormatDistributions = _metadataService.GetFormatDistributions(model.DistributionsFormats);

            if (ModelState.IsValid)
            {
                if (action.Equals(UI.Button_SaveAsXml))
                {
                    Stream fileStream = _metadataService.SaveMetadataAsXml(model);
                    var fileStreamResult = new FileStreamResult(fileStream, "application/xml")
                    {
                        FileDownloadName = model.Title + "_" + uuid + ".xml"
                    };

                    return fileStreamResult;
                }
                else if (action.Equals(UI.Button_Add_Distribution))
                {
                    model.DistributionsFormats.Add(new GeoNorgeAPI.SimpleDistribution {  FormatName ="", FormatVersion ="", Name = "", Organization = "", Protocol="", UnitsOfDistribution="", URL=""});
                    model.FormatDistributions = _metadataService.GetFormatDistributions(model.DistributionsFormats);
                    PrepareViewBagForEditing(model);
                    ViewBag.NewDistribution = true;
                    return View(model);
                }
                else if (action.Contains(UI.Button_Remove_Distribution))
                {
                    int deleteIndexStart = -1; int deleteIndexStop = -1;
                    var index = action.Split('-');
                    var indexStart = index[1];
                    var indexStop= index[2];
                    int.TryParse(indexStart, out deleteIndexStart);
                    int.TryParse(indexStop, out deleteIndexStop);
                    if (deleteIndexStart > -1 && deleteIndexStop > -1)
                    {
                        for (int i = deleteIndexStop; i >= deleteIndexStart; i--  )
                            model.DistributionsFormats.RemoveAt(i);
                    }
                            
                    model.FormatDistributions = _metadataService.GetFormatDistributions(model.DistributionsFormats);
                    PrepareViewBagForEditing(model);
                    return View(model);
                }
                else
                {
                    if (ignoreValidationError != "1")
                    {
                        foreach (var distro in model.FormatDistributions)
                        {
                            if (distro.Key.Protocol == null)
                            {
                                ModelState.AddModelError("distributionProtocolMissing", "Vennligst fyll ut distribusjonstype");
                                PrepareViewBagForEditing(model);
                                return View(model);
                            }
                        }
                    }

                    SaveMetadataToCswServer(model);
                    if (action.Equals(UI.Button_Validate))
                    {
                        ValidateMetadata(uuid);
                    }

                    return RedirectToAction("Edit", new { uuid = model.Uuid });
                }
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

        private void ValidateModel(MetadataViewModel model)
        {
            ViewBag.thumbnailMissingCSS = "";
            var thumb = model.Thumbnails.Where(t => t.Type == "thumbnail" || t.Type == "miniatyrbilde");
            if (thumb.Count() == 0) 
            { 
                ModelState.AddModelError("thumbnailMissing", "Det er påkrevd å fylle ut illustrasjonsbilde");
                ViewBag.thumbnailMissingCSS = "input-validation-error";
                }
        }

        [HttpGet]
        public ActionResult FlushCache()
        {
            MemoryCacher memCacher = new MemoryCacher();
            memCacher.DeleteAll();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public string GetSafeFilename(string filename)
        {
            filename = filename.Replace(" ", "_");
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        private void SaveMetadataToCswServer(MetadataViewModel model)
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

        public Dictionary<string, string> GetListOfRestrictionValuesAdjusted()
        {
            return GetCodeList("2BBCD2DF-C943-4D22-8E49-77D434C8A80D");

        }

        public Dictionary<string, string> GetListOfpredefinedDistributionProtocols()
        {

            return GetCodeList("94B5A165-7176-4F43-B6EC-1063F7ADE9EA");

        }


        public Dictionary<string, string> GetListOfNationalTheme()
        {
            return GetCodeList("42CECF70-0359-49E6-B8FF-0D6D52EBC73F");
        }

        public Dictionary<string, string> GetListOfNationalInitiative()
        {
            return GetCodeList("37204B11-4802-44B6-80A1-519968BD072F");
        }

        public Dictionary<string, string> GetListOfCatalogs()
        {
            Dictionary<string, string> catalogs = GetCodeList("65baf580-fee4-443c-8d6b-e5104280c4d4");
            catalogs.Remove("Inspire");

            return catalogs;
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

            if (CodeValues.Count < 1)
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

            CodeValues = CodeValues.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
            memCacher.Set(systemid, CodeValues, new DateTimeOffset(DateTime.Now.AddYears(1)));
            }

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

            if (Organizations.Count < 1)
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

                Organizations = Organizations.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
                memCacher.Set("organizations", Organizations, new DateTimeOffset(DateTime.Now.AddYears(1)));

            }

            return Organizations;
        }

        public Dictionary<string, string> GetListOfReferenceSystems()
        {
            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue("referencesystems");

            Dictionary<string, string> ReferenceSystems = new Dictionary<string, string>();

            if (cache != null)
            {
                ReferenceSystems = cache as Dictionary<string, string>;
            }

            if (ReferenceSystems.Count < 1)
            {

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
                memCacher.Set("referencesystems", ReferenceSystems, new DateTimeOffset(DateTime.Now.AddYears(1)));

            }

            return ReferenceSystems;
        }

        public Dictionary<string, string> GetRegister(string registername, MetadataViewModel model)
        {
            string role = GetSecurityClaim("role");

            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue("registeritem-"+ registername);

            List<RegisterItem> RegisterItems = new List<RegisterItem>();

            if (cache != null)
            {
                RegisterItems = cache as List<RegisterItem>;
            }

            if(RegisterItems.Count < 1)
            {

                System.Net.WebClient c = new System.Net.WebClient();
                c.Encoding = System.Text.Encoding.UTF8;
                var data = c.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/register/" + registername);
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var items = response["containeditems"];

                foreach (var item in items)
                {
                    var id = item["id"].ToString();
                    var owner = item["owner"].ToString();
                    string organization = item["owner"].ToString();

                    var registerItem = new RegisterItem { Id = id, Label = item["label"].ToString(), Organization = organization };

                    if (!RegisterItems.Contains(registerItem))
                    {
                       RegisterItems.Add(registerItem);
                    }
                }

                var logLines = RegisterItems.Select(l => l.Id + ": " + l.Label);
                Log.Info(string.Format("Setting cache for registername: {0}, items: "+ Environment.NewLine + " {1}", registername, string.Join(Environment.NewLine, logLines)));
                memCacher.Set("registeritem-" + registername, RegisterItems, new DateTimeOffset(DateTime.Now.AddYears(1)));

            }

            Dictionary<string, string> RegisterItemsForUser = new Dictionary<string, string>();

            foreach(var item in RegisterItems)
            {
                if (!string.IsNullOrWhiteSpace(role) && role.Equals("nd.metadata_admin") || model.HasAccess(item.Organization))
                {
                    RegisterItemsForUser.Add(item.Id, item.Label);
                }
            
            }

            RegisterItemsForUser = RegisterItemsForUser.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

            return RegisterItemsForUser;
        }

        public Dictionary<string, string> GetSubRegister(string registername, MetadataViewModel model)
        {
            string role = GetSecurityClaim("role");


            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue("subregisteritem");

            Dictionary<string, string> RegisterItems = new Dictionary<string, string>();

            if (cache != null)
            {
                RegisterItems = cache as Dictionary<string, string>;
            }

            if (RegisterItems.Count < 1)
            {

                System.Net.WebClient c = new System.Net.WebClient();
                c.Encoding = System.Text.Encoding.UTF8;
                var data = c.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/" + registername);
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var items = response["containeditems"];

                foreach (var item in items)
                {
                    var id = item["id"].ToString();
                    var owner = item["owner"].ToString();
                    string organization = item["owner"].ToString();

                    if (!RegisterItems.ContainsKey(id))
                    {
                            RegisterItems.Add(id, item["label"].ToString());
                    }
                }

                RegisterItems = RegisterItems.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
                memCacher.Set("subregisteritem", RegisterItems, new DateTimeOffset(DateTime.Now.AddYears(1)));
            }

            return RegisterItems;
        }


        public Dictionary<string, string> GetListOfLicenseTypes()
        {
            return GetCodeList("B7A92D72-7AB4-4C2C-8A01-516A0A00344A"); //Todo use uuid from production
        }

        [Authorize]
        [OutputCache(Duration = 0)]
        public ActionResult UploadThumbnail(string uuid, bool scaleImage = false)    
        {
            string filename = null;
            var viewresult = Json(new {});
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];

                if (file.ContentType == "image/jpeg" || file.ContentType == "image/gif" || file.ContentType == "image/png")
                {

                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    filename = uuid + "_" + timestamp + "_" + file.FileName;
                    string fullPath = Server.MapPath("~/thumbnails/" + filename);
             
                    if (scaleImage)
                    {
                        OptimizeImage(file, 180, 1000, fullPath);
                    }
                    else
                    {
                        file.SaveAs(fullPath);
                    }

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
        [OutputCache(Duration = 0)]
        public ActionResult UploadThumbnailGenerateMini(string uuid)
        {
            string filename = null;
            var viewresult = Json(new { });
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];

                if (file.ContentType == "image/jpeg" || file.ContentType == "image/gif" || file.ContentType == "image/png")
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    filename = uuid + "_" + timestamp + "_" + file.FileName;
                    string fullPath = Server.MapPath("~/thumbnails/" + filename);

                    file.SaveAs(fullPath);

                    var filenameMini = uuid + "_" + timestamp + "_mini_" + file.FileName;
                    var fullPathMini = Server.MapPath("~/thumbnails/" + filenameMini);

                    OptimizeImage(file, 180, 1000, fullPathMini);


                    viewresult = Json(new { status = "OK", filename = filename, filenamemini = filenameMini });
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
        [OutputCache(Duration = 0)]
        public ActionResult UploadThumbnailGenerateMiniAndMedium(string uuid)
        {
            string filename = null;
            var viewresult = Json(new { });
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];

                if (file.ContentType == "image/jpeg" || file.ContentType == "image/gif" || file.ContentType == "image/png")
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    filename = uuid + "_" + timestamp + "_" + file.FileName;
                    string fullPath = Server.MapPath("~/thumbnails/" + filename);

                    file.SaveAs(fullPath);

                    var filenameMini = uuid + "_" + timestamp + "_mini_" + file.FileName;
                    var fullPathMini = Server.MapPath("~/thumbnails/" + filenameMini);

                    OptimizeImage(file, 180, 1000, fullPathMini);

                    var filenameMedium = uuid + "_" + timestamp + "_medium_" + file.FileName;
                    var fullPathMedium = Server.MapPath("~/thumbnails/" + filenameMedium);

                    OptimizeImage(file, 300, 1000, fullPathMedium);


                    viewresult = Json(new { status = "OK", filename = filename, filenamemini = filenameMini, filenameMedium = filenameMedium });
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


        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }

        public static void OptimizeImage(HttpPostedFileBase file, int maxWidth, int maxHeight, string outputPath, int quality = 70)
        {
            ImageResizer.ImageJob newImage = 
                new ImageResizer.ImageJob(file, outputPath,
                new ImageResizer.Instructions("maxwidth=" + maxWidth + ";maxheight=" + maxHeight + ";quality=" + quality));

            newImage.Build();
        }

        [Authorize]
        [HttpGet]
        public ActionResult ConfirmDelete(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return HttpNotFound();

            MetadataViewModel model = _metadataService.GetMetadataModel(uuid);

            string role = GetSecurityClaim("role");
            if (HasAccessToMetadata(model))
            {
                return View(model);
            } else {
                return new HttpUnauthorizedResult();
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Delete(string uuid)
        {
            MetadataViewModel model = _metadataService.GetMetadataModel(uuid);

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

        private bool HasAccessToMetadata(MetadataViewModel model)
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
            ViewBag.RegisterOrganizationUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/search?facets[0]name=organization&facets[0]value=" + organization;
            return View();
        
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }

	}
    
    public enum MetadataMessages
    {
        InvalidUuid
    }

    class RegisterItem
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Organization { get; set; }
    }
}