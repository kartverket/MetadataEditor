using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Kartverket.MetadataEditor.Models
{
    public class WmsServiceParser
    {

        public List<WmsLayerViewModel> GetLayers(string wmsUrl)
        {
            wmsUrl = wmsUrl + "SERVICE=WMS&VERSION=1.1.1&REQUEST=GetCapabilities";

            XDocument xd = XDocument.Load(wmsUrl);

            IEnumerable<XElement> layers =
                    from el in xd.Elements("WMT_MS_Capabilities").Elements("Capability").Elements("Layer").Elements("Layer")
                    select el;

            List<WmsLayerViewModel> models = new List<WmsLayerViewModel>();

            foreach (var layer in layers)
            {
                var boundingBox = layer.Element("LatLonBoundingBox");
                var nameElement = layer.Element("Name");
                var titleElement = layer.Element("Title");
                models.Add(new WmsLayerViewModel
                {
                    Name = nameElement != null ? nameElement.Value : null,
                    Title = titleElement != null ? titleElement.Value : null,
                    BoundingBoxEast = boundingBox != null ? boundingBox.Attribute("maxx").Value : null,
                    BoundingBoxWest = boundingBox != null ? boundingBox.Attribute("minx").Value : null,
                    BoundingBoxNorth = boundingBox != null ? boundingBox.Attribute("maxy").Value : null,
                    BoundingBoxSouth = boundingBox != null ? boundingBox.Attribute("miny").Value : null,
                });
            }
            return models;
        }

    }
}