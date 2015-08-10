using Kartverket.MetadataEditor.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Controllers
{
    public class ValidatorController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));


        // GET: Validator
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ValidateAll()
        {
            Log.Info("Start validating all metadata.");

            new Thread(() => new ValidatorService().ValidateAllMetadata()).Start();

            TempData["message"] = "Validering og sending av epost startet!";

            return RedirectToAction("Index");
        }

    }
}