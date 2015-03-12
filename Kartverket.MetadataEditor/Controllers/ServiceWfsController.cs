using Kartverket.MetadataEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Controllers
{
    [Authorize]
    public class ServiceWfsController : Controller
    {
        private MetadataService _metadataService;
        private WfsServiceParser _wfsServiceParser;

        public ServiceWfsController()
        {
            _metadataService = new MetadataService();
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

        private string GetUsername()
        {
            return GetSecurityClaim("username");
        }

        private string GetSecurityClaim(string type)
        {
            string result = null;
            foreach (var claim in System.Security.Claims.ClaimsPrincipal.Current.Claims)
            {
                if (claim.Type == type && !string.IsNullOrWhiteSpace(claim.Value))
                {
                    result = claim.Value;
                    break;
                }
            }

            // bad hack, must fix BAAT
            if (!string.IsNullOrWhiteSpace(result) && type.Equals("organization") && result.Equals("Statens kartverk"))
            {
                result = "Kartverket";
            }

            return result;
        }
	}
}