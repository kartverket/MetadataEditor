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


        public ActionResult Index(MetadataMessages? message)
        {
            ViewBag.StatusMessage =
                message == MetadataMessages.InvalidUuid ? Resources.UI.Error_InvalidUuid
                : "";

            var model = new MetadataIndexViewModel();

            string organization = "Skog og landskap";
            /*
            if (User.Identity.IsAuthenticated)
            {
                foreach (var claim in System.Security.Claims.ClaimsPrincipal.Current.Claims)
                {
                    Log.Info(string.Format("Claim type={0}, value={1}", claim.Type, claim.Value));
                    if (claim.Type == "organization" && !string.IsNullOrWhiteSpace(claim.Value))
                    {
                        organization = claim.Value;
                    }
                }
            }*/

            model.MetadataItems = _metadataService.GetMyMetadata("Kartverket");
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