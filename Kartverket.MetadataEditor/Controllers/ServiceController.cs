using Kartverket.MetadataEditor.Helpers;
using Kartverket.MetadataEditor.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Controllers
{
    [HandleError]
    [Authorize]
    public class ServiceController : ControllerBase
    {
        private WmsServiceParser _wmsServiceParser;

        private IMetadataService _metadataService;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ServiceController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
            _wmsServiceParser = new WmsServiceParser();
        }

        public ActionResult Index(string uuid, string wmsUrl)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return HttpNotFound();

            MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);
            if (string.IsNullOrWhiteSpace(wmsUrl))
            {
                wmsUrl = metadata.DistributionUrl;
            }

            WmsServiceViewModel serviceModel = null;
            try
            {
                serviceModel = _wmsServiceParser.GetLayers(wmsUrl);
            }
            catch (Exception e)
            {
                ViewBag.Message = "Feil ved henting av GetCapabilities: " + e.Message;
            }
            
            ServiceLayerViewModel model = new ServiceLayerViewModel
            {
                Metadata = metadata,
                Layers = serviceModel != null ? serviceModel.Layers : new List<WmsLayerViewModel>(),
                WmsUrl = wmsUrl,
            };

            return View(model);
        }

        public ActionResult CreateMetadataForLayers(string uuid, string wmsUrl, String[] selectedLayers, string[] keywords)
        {
            MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);

            WmsServiceViewModel serviceModel = _wmsServiceParser.GetLayers(wmsUrl);

            List<WmsLayerViewModel> createMetadataForLayers = new List<WmsLayerViewModel>();

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
            List<WmsLayerViewModel> newlyCreatedLayerMetadata = _metadataService.CreateMetadataForLayers(uuid, createMetadataForLayers, keywords, username);

            // Creates dataset metadata in bulk without going through MetadataController.Create, so
            // it would otherwise be invisible. The service URL is left out - it can be a host that
            // is not public.
            TelemetryHelper.Capture(TempData, "metadataeditor_service_layers_created", new Dictionary<string, object>
            {
                { "editor", "full" },
                { "service_type", "wms" },
                { "layer_count", newlyCreatedLayerMetadata != null ? newlyCreatedLayerMetadata.Count : 0 },
                { "selected_layer_count", selectedLayers != null ? selectedLayers.Length : 0 },
                { "keyword_count", keywords != null ? keywords.Length : 0 }
            });

            ServiceLayerViewModel model = new ServiceLayerViewModel
            {
                Metadata = metadata,
                Layers = newlyCreatedLayerMetadata,
                WmsUrl = wmsUrl,
            };

            return View("LayersCreated", model);
        }

      
        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
	}
}