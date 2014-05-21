using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Kartverket.MetadataEditor.Models
{
    public class WmsServiceParser
    {
        private static XNamespace WMS = "http://www.opengis.net/wms";
        private static XNamespace INSPIRE = "http://inspire.ec.europa.eu/schemas/inspire_vs/1.0";
        private static XNamespace INSPIRE_COMMON = "http://inspire.ec.europa.eu/schemas/common/1.0";

        public WmsServiceViewModel GetLayers(string wmsUrl)
        {
            WmsServiceViewModel serviceModel = new WmsServiceViewModel();

            if (!wmsUrl.EndsWith("?"))
            {
                wmsUrl = wmsUrl + "?";
            }
            wmsUrl = wmsUrl + "service=wms&request=GetCapabilities";

            XDocument xd = XDocument.Load(wmsUrl);

            XElement root = xd.Element(WMS + "WMS_Capabilities");
            if (root != null)
            {
                WmsGetCapabilities13Parser parser = new WmsGetCapabilities13Parser();
                serviceModel = parser.Parse(xd);
                if (serviceModel.SupportsEnglishGetCapabilities)
                {
                    XDocument getCapabilitiesEnglish = XDocument.Load(wmsUrl + "&language=eng");
                    Dictionary<string, Dictionary<string, string>> englishInformation = parser.ParseEnglishGetCapabilities(getCapabilitiesEnglish);
                    serviceModel.AppendEnglishInformation(englishInformation);
                }
            }
            else
            {
                root = xd.Element("WMT_MS_Capabilities");
                if (root != null)
                {
                    serviceModel.Layers = parseGetCapabilitiesV1_1_1(root);
                }
            }

            return serviceModel;
        }

        private List<WmsLayerViewModel> parseGetCapabilitiesV1_1_1(XElement root)
        {
            IEnumerable<XElement> layers =
                    from el in root.Elements("Capability").Elements("Layer").Elements("Layer")
                    select el;

            List<WmsLayerViewModel> models = new List<WmsLayerViewModel>();

            foreach (var layer in layers)
            {
               // var boundingBox = layer.Element("EX_GeographicBoundingBox");
                var nameElement = layer.Element("Name");
                var titleElement = layer.Element("Title");
                var abstractElement = layer.Element("Abstract");
                List<string> keywords = ParseKeywords(layer.Element("KeywordList"));

                models.Add(new WmsLayerViewModel
                {
                    Name = nameElement != null ? nameElement.Value : null,
                    Title = titleElement != null ? titleElement.Value : null,
                    Abstract = abstractElement != null ? abstractElement.Value : null,
                    //BoundingBoxEast = boundingBox != null ? boundingBox.Element(WMS + "eastBoundLongitude").Value : null,
                    //BoundingBoxWest = boundingBox != null ? boundingBox.Element(WMS + "westBoundLongitude").Value : null,
                    //BoundingBoxNorth = boundingBox != null ? boundingBox.Element(WMS + "northBoundLatitude").Value : null,
                    //BoundingBoxSouth = boundingBox != null ? boundingBox.Element(WMS + "southBoundLatitude").Value : null,
                    Keywords = keywords,
                });
            }

            return models;
        }

        private static List<string> ParseKeywords(XElement keywordListElement)
        {
            List<string> keywords = new List<string>();
            if (keywordListElement != null)
            {
                var keywordListValues = keywordListElement.Elements("Keyword");
                foreach (var keyword in keywordListValues)
                {
                    var keywordValue = keyword.Value;
                    if (!string.IsNullOrWhiteSpace(keywordValue))
                    {
                        keywords.Add(keywordValue);
                    }
                }
            }
            return keywords;
        }






    }
}