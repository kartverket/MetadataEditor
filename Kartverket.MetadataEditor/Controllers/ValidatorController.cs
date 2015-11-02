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
        [Authorize]
        public ActionResult ValidateAll()
        {
            Log.Info("Start validating all metadata.");

            //new Thread(() => new ValidatorService().ValidateAllMetadata()).Start();
            List<string> EmailsTo = new ValidatorService().ValidateAllMetadata();

            //TempData["message"] = "Validering og sending av epost startet!";

            return View("Index", EmailsTo);
        }


        [HttpPost]
        [Authorize]
        public ActionResult SendEmail(FormCollection form)
        {
            Log.Info("Start sending email");

            List<string> emails = GetFormData(form);

            new Thread(() => new ValidatorService().SendEmail(emails)).Start();

            TempData["message"] = "Sender epost";

            return View("Index");
        }


        private List<string> GetFormData(FormCollection form)
        {

            List<string> emails = new List<string>();
            if (form["emails"] != null)
            {
                emails = form["emails"].Split(',').ToList();
            }

            return emails;

        }

     }
}