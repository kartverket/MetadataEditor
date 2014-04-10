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

        public List<WmsLayerViewModel> GetLayers(string wmsUrl)
        {
            wmsUrl = wmsUrl + "service=wms&request=GetCapabilities";

            XDocument xd = XDocument.Load(wmsUrl);

            

            IEnumerable<XElement> layers =
                    from el in xd.Elements(WMS + "WMS_Capabilities").Elements(WMS + "Capability").Elements(WMS + "Layer").Elements(WMS + "Layer")
                    select el;

            List<WmsLayerViewModel> models = new List<WmsLayerViewModel>();

            foreach (var layer in layers)
            {
                var boundingBox = layer.Element(WMS + "EX_GeographicBoundingBox");
                var nameElement = layer.Element(WMS +"Name");
                var titleElement = layer.Element(WMS + "Title");
                var abstractElement = layer.Element(WMS + "Abstract");
                List<string> keywords = ParseKeywords(layer.Element(WMS + "KeywordList"));

                models.Add(new WmsLayerViewModel
                {
                    Name = nameElement != null ? nameElement.Value : null,
                    Title = titleElement != null ? titleElement.Value : null,
                    Abstract = abstractElement != null ? abstractElement.Value : null,
                    BoundingBoxEast = boundingBox != null ? boundingBox.Element(WMS + "eastBoundLongitude").Value : null,
                    BoundingBoxWest = boundingBox != null ? boundingBox.Element(WMS + "westBoundLongitude").Value : null,
                    BoundingBoxNorth = boundingBox != null ? boundingBox.Element(WMS + "northBoundLatitude").Value : null,
                    BoundingBoxSouth = boundingBox != null ? boundingBox.Element(WMS + "southBoundLatitude").Value : null,
                    Keywords = keywords
                });
            }
            return models;
        }



        private static List<string> ParseKeywords(XElement keywordListElement)
        {
            List<string> keywords = new List<string>();
            if (keywordListElement != null)
            {
                var keywordListValues = keywordListElement.Elements(WMS + "Keyword");
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