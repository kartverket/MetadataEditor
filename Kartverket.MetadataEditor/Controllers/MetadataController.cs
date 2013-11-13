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
        public ActionResult Index()
        {
            var model = new MetadataIndexViewModel();
            model.MetadataItems = new MetadataService().GetMyMetadata("Kartverket");
            return View(model);
        }
	}
}