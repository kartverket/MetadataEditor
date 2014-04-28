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

        public ActionResult Index(MetadataMessages? message, int offset = 1, int limit = 50)
        {
            ViewBag.StatusMessage =
                message == MetadataMessages.InvalidUuid ? Resources.UI.Error_InvalidUuid
                : "";

            var model = new MetadataIndexViewModel();

            if (User.Identity.IsAuthenticated)
            {
                string organization = "";
                foreach (var claim in System.Security.Claims.ClaimsPrincipal.Current.Claims)
                {
                    Log.Info(string.Format("Claim type={0}, value={1}", claim.Type, claim.Value));
                    if (claim.Type == "organization" && !string.IsNullOrWhiteSpace(claim.Value))
                    {
                        organization = claim.Value;
                    }
                }
                if (!string.IsNullOrWhiteSpace(organization))
                {
                    if (organization == "Statens kartverk")
                        organization = "Kartverket";

                    model = _metadataService.GetMyMetadata(organization, offset, limit);
                }
            }
            return View(model);
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

	}

    public enum MetadataMessages
    {
        InvalidUuid
    }
}