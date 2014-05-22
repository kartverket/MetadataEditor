using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Kartverket.MetadataEditor.Models
{
    public class WmsGetCapabilities11Parser
    {
        public WmsServiceViewModel Parse(XDocument getCapabilitiesXmlDocument)
        {
            WmsServiceViewModel serviceModel = new WmsServiceViewModel();

            IEnumerable<XElement> layers =
                    from el in getCapabilitiesXmlDocument.Element("WMT_MS_Capabilities").Elements("Capability").Elements("Layer").Elements("Layer")
                    select el;

            foreach (var layer in layers)
            {
                serviceModel.Layers.AddRange(ParseLayerData(layer));
            }

            return serviceModel;
        }

        private List<WmsLayerViewModel> ParseLayerData(XElement layer)
        {
            List<WmsLayerViewModel> parsedLayers = new List<WmsLayerViewModel>();
            
            var nameElement = layer.Element("Name");
            var titleElement = layer.Element("Title");
            var abstractElement = layer.Element("Abstract");
            List<string> keywords = ParseKeywords(layer.Element("KeywordList"));
            var boundingBox = layer.Element("LatLonBoundingBox");

            string boundingBoxEast = null;
            string boundingBoxWest = null;
            string boundingBoxNorth = null;
            string boundingBoxSouth = null;

            if (boundingBox != null)
            {
                if (boundingBox.Attribute("maxx") != null)
                {
                    boundingBoxEast = boundingBox.Attribute("maxx").Value;
                }
                if (boundingBox.Attribute("maxy") != null)
                {
                    boundingBoxNorth = boundingBox.Attribute("maxy").Value;
                }
                if (boundingBox.Attribute("minx") != null)
                {
                    boundingBoxWest = boundingBox.Attribute("minx").Value;
                }
                if (boundingBox.Attribute("miny") != null)
                {
                    boundingBoxSouth = boundingBox.Attribute("miny").Value;
                }
            }

            IEnumerable<XElement> subLayers = from el in layer.Elements("Layer") select el;

            parsedLayers.Add(new WmsLayerViewModel
            {
                Name = nameElement != null ? nameElement.Value : null,
                Title = titleElement != null ? titleElement.Value : null,
                Abstract = abstractElement != null ? abstractElement.Value : null,
                BoundingBoxEast = boundingBoxEast,
                BoundingBoxWest = boundingBoxWest,
                BoundingBoxNorth = boundingBoxNorth,
                BoundingBoxSouth = boundingBoxSouth,
                Keywords = keywords,
                IsGroupLayer = subLayers != null && subLayers.Count() > 0
            });

            if (subLayers != null && subLayers.Count() > 0)
            {
                foreach (var subLayer in subLayers)
                {
                    parsedLayers.AddRange(ParseLayerData(subLayer));
                }
            }

            return parsedLayers;
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