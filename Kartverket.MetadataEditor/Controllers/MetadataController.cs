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



	}

    public enum MetadataMessages
    {
        InvalidUuid
    }
}