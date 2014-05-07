using Kartverket.MetadataEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Controllers
{
    public class MetadataController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private MetadataService _metadataService;

        public MetadataController()
        {
            _metadataService = new MetadataService();
        }

        [HttpGet]
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
        public ActionResult Create(MetadataCreateViewModel model)
        {
            model.MetadataContactOrganization = GetSecurityClaim("organization");
            if (ModelState.IsValid)
            {
                string uuid = _metadataService.CreateMetadata(model);

                return RedirectToAction("Edit", new { uuid = uuid });
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
                    model = _metadataService.SearchMetadata(userOrganization, null, offset, limit);
                }

                model.UserOrganization = userOrganization;
            }
            return View(model);
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
        public ActionResult Edit(string uuid, bool saved = false)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return HttpNotFound();

            MetadataViewModel model = _metadataService.GetMetadataModel(uuid);

            ViewBag.TopicCategoryValues = new SelectList(GetListOfTopicCategories(), "Key", "Value", model.TopicCategory);
            ViewBag.SpatialRepresentationValues = new SelectList(GetListOfSpatialRepresentations(), "Key", "Value", model.SpatialRepresentation);
            ViewBag.MaintenanceFrequencyValues = new SelectList(GetListOfMaintenanceFrequencyValues(), "Key", "Value", model.MaintenanceFrequency);
            ViewBag.StatusValues = new SelectList(GetListOfStatusValues(), "Key", "Value", model.Status);
            ViewBag.SecurityConstraintValues = new SelectList(GetListOfClassificationValues(), "Key", "Value", model.SecurityConstraints);
            ViewBag.UseConstraintValues = new SelectList(GetListOfRestrictionValues(), "Key", "Value", model.UseConstraints);
            ViewBag.AccessConstraintValues = new SelectList(GetListOfRestrictionValues(), "Key", "Value", model.AccessConstraints);
            ViewBag.Saved = saved;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(MetadataViewModel model)
        {
            _metadataService.SaveMetadataModel(model);
            return RedirectToAction("Edit", new { uuid = model.Uuid, saved = true});
        }

        public Dictionary<string, string> GetListOfTopicCategories()
        {
            return new Dictionary<string, string> 
            {
                {"farming", "farming"}, 
                {"biota", "biota"},
                {"boundaries", "boundaries"},
                {"climatologyMeteorologyAtmosphere","climatologyMeteorologyAtmosphere"},
                {"economy","economy"},
                {"elevation","elevation"},
                {"environment","environment"},
                {"geoscientificInformation","geoscientificInformation"},
                {"health","health"},
                {"imageryBaseMapsEarthCover","imageryBaseMapsEarthCover"},
                {"intelligenceMilitary","intelligenceMilitary"},
                {"inlandWaters","inlandWaters"},
                {"location","location"},
                {"oceans","oceans"},
                {"planningCadastre","planningCadastre"},
                {"society","society"},
                {"structure","structure"},
                {"transportation","transportation"},
                {"utilitiesCommunication","utilitiesCommunication"},
            };
        }

        public Dictionary<string, string> GetListOfSpatialRepresentations()
        {
            return new Dictionary<string, string> 
            {
                {"vector", "vector"}, 
                {"grid", "grid"}, 
                {"textTable", "textTable"}, 
                {"tin", "tin"}, 
                {"stereoModel", "stereoModel"}, 
                {"video", "video"}, 
            };
        }

        public Dictionary<string, string> GetListOfMaintenanceFrequencyValues()
        {
            return new Dictionary<string, string>
            {
                {"continual", "Kontinuerlig"},
                {"daily", "Daglig"},
                {"weekly", "Ukentlig"},
                {"fortnightly", "Annenhver uke"},
                {"monthly", "Månedlig"},
                {"quarterly", "Hvert kvartal"},
                {"biannually", "Hvert halvår"},
                {"annually", "Årlig"},
                {"asNeeded", "Etter behov"},
                {"irregular", "Ujevnt"},
                {"notPlanned", "Ikke planlagt"},
                {"unknown", "Ukjent"},
            };
        }

        public Dictionary<string, string> GetListOfStatusValues()
        {
            return new Dictionary<string, string>
            {
                {"completed", "Fullført"},
                {"historicalArchive", "Arkivert"},
                {"obsolete", "Utdatert"},
                {"onGoing", "Kontinuerlig oppdatert"},
                {"planned", "Planlagt"},
                {"required", "Må oppdateres"},
                {"underDevelopment", "Under arbeid"},
            };
        }

        public Dictionary<string, string> GetListOfClassificationValues()
        {
            return new Dictionary<string, string>
            {
                {"unclassified", "unclassified"},
                {"restricted", "restricted"},
                {"confidential", "confidential"},
                {"secret", "secret"},
                {"topSecret", "topSecret"},
            };
        }

        public Dictionary<string, string> GetListOfRestrictionValues()
        {
            return new Dictionary<string, string>
            {
                {"copyright", "copyright"},
                {"patent", "patent"},
                {"patentPending", "patentPending"},
                {"trademark", "trademark"},
                {"license", "license"},
                {"restricted", "restricted"},
                {"otherRestrictions", "otherRestrictions"},
            };
        }

        public ActionResult UploadThumbnail(string uuid)
        {
            string filename = null;
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];
                filename = uuid + "_" + file.FileName;
                string fullPath = Server.MapPath("~/Content/thumbnails/" + filename);
                file.SaveAs(fullPath);
            }

            var viewresult = Json(new { status = "OK", filename = filename});
            //for IE8 which does not accept application/json
            if (Request.Headers["Accept"] != null && !Request.Headers["Accept"].Contains("application/json"))
                viewresult.ContentType = "text/plain";

            return viewresult;
        }

	}

    public enum MetadataMessages
    {
        InvalidUuid
    }
}