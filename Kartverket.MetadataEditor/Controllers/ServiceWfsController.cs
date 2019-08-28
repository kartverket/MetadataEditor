using Kartverket.MetadataEditor.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Controllers
{
    [HandleError]
    [Authorize]
    public class ServiceWfsController : ControllerBase
    {
        private IMetadataService _metadataService;
        private WfsServiceParser _wfsServiceParser;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ServiceWfsController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
            _wfsServiceParser = new WfsServiceParser();
        }

        public ActionResult Index(string uuid, string wfsUrl)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return HttpNotFound();

            MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
            if (string.IsNullOrWhiteSpace(wfsUrl))
            {
                wfsUrl = metadata.DistributionUrl;
            }

            WfsServiceViewModel serviceModel = null;
            try
            {
                serviceModel = _wfsServiceParser.GetLayers(wfsUrl);
            }
            catch (Exception e)
            {
                ViewBag.Message = "Feil ved henting av GetCapabilities: " + e.Message;
            }
            
            WfsServiceLayerViewModel model = new WfsServiceLayerViewModel
            {
                Metadata = metadata,
                Layers = serviceModel != null ? serviceModel.Layers : new List<WfsLayerViewModel>(),
                WfsUrl = wfsUrl,
            };

            return View(model);
        }

        public ActionResult CreateMetadataForLayers(string uuid, string wfsUrl, String[] selectedLayers, string[] keywords)
        {
            MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);

            WfsServiceViewModel serviceModel = _wfsServiceParser.GetLayers(wfsUrl);

            List<WfsLayerViewModel> createMetadataForLayers = new List<WfsLayerViewModel>();

            if (selectedLayers != null) {
                foreach (var layer in serviceModel.Layers)
                {
                    if (selectedLayers.Contains(layer.Name))
                    {
                        createMetadataForLayers.Add(layer);
                    }
                }
            }
            string username = GetUsername();
            List<WfsLayerViewModel> newlyCreatedLayerMetadata = _metadataService.CreateMetadataForFeature(uuid, createMetadataForLayers, keywords, username);
            
            WfsServiceLayerViewModel model = new WfsServiceLayerViewModel
            {
                Metadata = metadata,
                Layers = newlyCreatedLayerMetadata,
                WfsUrl = wfsUrl,
            };

            return View("LayersCreated", model);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
	}
}