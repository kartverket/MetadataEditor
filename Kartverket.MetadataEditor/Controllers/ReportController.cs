using Kartverket.MetadataEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Controllers
{
    public class ReportController : Controller
    {
        private IReportService _reportService;
        public ReportController(IReportService reportService)
        {
           _reportService = reportService;
        }
        // GET: Report
        [Authorize]
        public ActionResult Index()
        {
            List<MetadataViewModel> metadata = _reportService.Report1();
            return View(metadata);
        }
    }
}