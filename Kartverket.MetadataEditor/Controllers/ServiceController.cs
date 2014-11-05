using Kartverket.MetadataEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Controllers
{
    [Authorize]
    public class ServiceController : Controller
    {
        private MetadataService _metadataService;
        private WmsServiceParser _wmsServiceParser;

        public ServiceController()
        {
            _metadataService = new MetadataService();
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
            List<WmsLayerViewModel> newlyCreatedLayerMetadata = _metadataService.CreateMetadataForLayers(uuid, createMetadataForLayers, keywords);
            
            ServiceLayerViewModel model = new ServiceLayerViewModel
            {
                Metadata = metadata,
                Layers = newlyCreatedLayerMetadata,
                WmsUrl = wmsUrl,
            };

            return View("LayersCreated", model);
        }

	}
}