using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Kartverket.MetadataEditor.Models
{
    public class WfsGetCapabilities10Parser
    {
        private static XNamespace WFS = "http://www.opengis.net/wfs";
        private static XNamespace INSPIRE = "http://inspire.ec.europa.eu/schemas/inspire_vs/1.0";
        private static XNamespace INSPIRE_COMMON = "http://inspire.ec.europa.eu/schemas/common/1.0";

        private static XNamespace ows = "http://www.opengis.net/ows";

        public WfsServiceViewModel Parse(XDocument getCapabilitiesXmlDocument)
        {
            WfsServiceViewModel serviceModel = new WfsServiceViewModel();

            XElement root = getCapabilitiesXmlDocument.Element(WFS + "WFS_Capabilities");


            //Implement support english later
            //serviceModel.SupportsEnglishGetCapabilities = CheckSupportForEnglishGetCapabilities(root);

            IEnumerable<XElement> layers =
                    from el in root.Elements(WFS + "FeatureTypeList").Elements(WFS + "FeatureType")
                    select el;

            List<WfsLayerViewModel> layerModels = new List<WfsLayerViewModel>();

            foreach (var layer in layers)
            {
                layerModels.AddRange(ParseLayerData(layer));
            }

            serviceModel.Layers = layerModels;

            return serviceModel;
        }

        private List<WfsLayerViewModel> ParseLayerData(XElement layer)
        {
            List<WfsLayerViewModel> parsedLayers = new List<WfsLayerViewModel>();

            var boundingBox = layer.Element(WFS + "LatLongBoundingBox");

            string WestBoundLongitude = null, SouthBoundLatitude = null, EastBoundLongitude = null, NorthBoundLatitude = null;

            if (boundingBox != null)
            {
                WestBoundLongitude = boundingBox.Attribute("minx").Value;
                SouthBoundLatitude = boundingBox.Attribute("miny").Value;
                EastBoundLongitude = boundingBox.Attribute("maxx").Value;
                NorthBoundLatitude = boundingBox.Attribute("maxy").Value;
            }

            var nameElement = layer.Element(WFS + "Name");
            var titleElement = layer.Element(WFS + "Title");
            var abstractElement = layer.Element(WFS + "Abstract");
            string keys = layer.Element(WFS + "Keywords").Value;
            keys = keys.Replace("\n", "");
            List<string> keywords = keys.Split(' ').ToList();
            keywords = keywords.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            string name = nameElement != null ? nameElement.Value : null;


            parsedLayers.Add(new WfsLayerViewModel
            {
                Name = name,
                Title = titleElement != null ? titleElement.Value : null,
                Abstract = abstractElement != null ? abstractElement.Value : null,
                BoundingBoxEast = !string.IsNullOrEmpty(EastBoundLongitude) ? EastBoundLongitude : null,
                BoundingBoxWest = !string.IsNullOrEmpty(WestBoundLongitude) ? WestBoundLongitude : null,
                BoundingBoxNorth = !string.IsNullOrEmpty(NorthBoundLatitude) ? NorthBoundLatitude : null,
                BoundingBoxSouth = !string.IsNullOrEmpty(SouthBoundLatitude) ? SouthBoundLatitude : null,
                Keywords = keywords,
                IsGroupLayer = false
            });


            return parsedLayers;
        }

        //not implemented
        private bool CheckSupportForEnglishGetCapabilities(XElement root)
        {
            bool supportsEnglishGetCapablities = false;

            return supportsEnglishGetCapablities;
        }

        //not implemented
        public Dictionary<string, Dictionary<string, string>> ParseEnglishGetCapabilities(XDocument getCapabilitiesEnglish)
        {

            Dictionary<string, Dictionary<string, string>> englishData = new Dictionary<string, Dictionary<string, string>>();

            return englishData;
        }


    }
}