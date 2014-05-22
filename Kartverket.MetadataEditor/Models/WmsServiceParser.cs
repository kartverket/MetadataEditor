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

            XDocument xmlDocument = XDocument.Load(wmsUrl);

            XElement root = xmlDocument.Element(WMS + "WMS_Capabilities");
            if (root != null)
            {
                WmsGetCapabilities13Parser parser = new WmsGetCapabilities13Parser();
                serviceModel = parser.Parse(xmlDocument);
                if (serviceModel.SupportsEnglishGetCapabilities)
                {
                    XDocument getCapabilitiesEnglish = XDocument.Load(wmsUrl + "&language=eng");
                    Dictionary<string, Dictionary<string, string>> englishInformation = parser.ParseEnglishGetCapabilities(getCapabilitiesEnglish);
                    serviceModel.AppendEnglishInformation(englishInformation);
                }
            }
            else
            {
                root = xmlDocument.Element("WMT_MS_Capabilities");
                if (root != null)
                {
                    serviceModel = new WmsGetCapabilities11Parser().Parse(xmlDocument);
                }
            }

            return serviceModel;
        }
    }
}