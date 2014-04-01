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
                models.Add(new WmsLayerViewModel
                {
                    Name = layer.Element("Name").Value,
                    Title = layer.Element("Title").Value,
                    BoundingBoxEast = boundingBox.Attribute("maxx").Value,
                    BoundingBoxWest = boundingBox.Attribute("minx").Value,
                    BoundingBoxNorth = boundingBox.Attribute("maxy").Value,
                    BoundingBoxSouth = boundingBox.Attribute("miny").Value,
                });
            }
            return models;
        }

    }
}