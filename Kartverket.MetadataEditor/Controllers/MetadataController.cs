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
            _metadataService = new MetadataService(new GeoNorgeAPI.GeoNorge("", "", "https://www.geonorge.no/geonetworkbeta/"));
        }


        public ActionResult Index(MetadataMessages? message, int offset = 1, int limit = 200)
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
        public ActionResult Edit(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return HttpNotFound();


            MetadataViewModel model = _metadataService.GetMetadataModel(uuid);

            ViewBag.TopicCategoryValues = new SelectList(GetListOfTopicCategories(), "Key", "Value", model.TopicCategory);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(MetadataViewModel model)
        {
            _metadataService.SaveMetadataModel(model);
            return RedirectToAction("Index");
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
            /*
farming
biota
boundaries
climatologyMeteorologyAtmosphere
economy
elevation
environment
geoscientificInformation
health
imageryBaseMapsEarthCover
intelligenceMilitary
inlandWaters
location
oceans
planningCadastre
society
structure
transportation
utilitiesCommunication
            */
        }


	}

    public enum MetadataMessages
    {
        InvalidUuid
    }
}