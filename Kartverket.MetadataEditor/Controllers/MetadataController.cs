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
            _metadataService = new MetadataService(new GeoNorgeAPI.GeoNorge("arkitektum", "geoportal3800", "https://www.geonorge.no/geonetworkbeta/"));
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
            
            /*
            if (User.Identity.IsAuthenticated)
            {
                model.MetadataItems = _metadataService.GetMyMetadata("Kartverket");
            }
            else
            {
                model.MetadataItems = new List<MetadataItemViewModel>();
            }
            */
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid))
             //   return RedirectToAction("Index", new { message = MetadataMessages.InvalidUuid });
                return HttpNotFound();


            MetadataViewModel model = _metadataService.GetMetadataModel(uuid);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(MetadataViewModel model)
        {
            _metadataService.SaveMetadataModel(model);
            return RedirectToAction("Index");
        }
        
	}

    public enum MetadataMessages
    {
        InvalidUuid
    }
}