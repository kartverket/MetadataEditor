using Kartverket.MetadataEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Controllers
{
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
            List<WmsLayerViewModel> layers = _wmsServiceParser.GetLayers(wmsUrl);

            ServiceLayerViewModel model = new ServiceLayerViewModel
            {
                Metadata = metadata,
                Layers = layers,
                WmsUrl = wmsUrl,
            };

            return View(model);
        }

        public ActionResult CreateMetadataForLayers(string uuid, string wmsUrl, String[] selectedLayers)
        {
            MetadataViewModel metadata = _metadataService.GetMetadataModel(uuid);

            List<WmsLayerViewModel> layersFromService = _wmsServiceParser.GetLayers(wmsUrl);

            List<WmsLayerViewModel> createMetadataForLayers = new List<WmsLayerViewModel>();
            foreach (var layer in layersFromService)
            {
                if (selectedLayers.Contains(layer.Name))
                {
                    createMetadataForLayers.Add(layer);
                }
            }

            List<WmsLayerViewModel> newlyCreatedLayerMetadata = _metadataService.CreateMetadataForLayers(uuid, createMetadataForLayers);
            
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