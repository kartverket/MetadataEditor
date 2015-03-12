using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Kartverket.MetadataEditor.Models
{
    public class WfsServiceParser
    {
        private static XNamespace WFS = "http://www.opengis.net/wfs";
        private static XNamespace INSPIRE = "http://inspire.ec.europa.eu/schemas/inspire_vs/1.0";
        private static XNamespace INSPIRE_COMMON = "http://inspire.ec.europa.eu/schemas/common/1.0";

        public WfsServiceViewModel GetLayers(string wfsUrl)
        {
            if (!string.IsNullOrWhiteSpace(wfsUrl))
            {
                WfsServiceViewModel serviceModel = new WfsServiceViewModel();

                if (!wfsUrl.EndsWith("?"))
                {
                    wfsUrl = wfsUrl + "?";
                }
                wfsUrl = wfsUrl + "service=wfs&request=GetCapabilities";

                XDocument xmlDocument = XDocument.Load(wfsUrl);

                XElement root = xmlDocument.Element(WFS + "WFS_Capabilities");
                if (root != null)
                {
                    WfsGetCapabilities11Parser parser = new WfsGetCapabilities11Parser();
                    serviceModel = parser.Parse(xmlDocument);
                    if (serviceModel.SupportsEnglishGetCapabilities)
                    {
                        XDocument getCapabilitiesEnglish = XDocument.Load(wfsUrl + "&language=eng");
                        Dictionary<string, Dictionary<string, string>> englishInformation = parser.ParseEnglishGetCapabilities(getCapabilitiesEnglish);
                        serviceModel.AppendEnglishInformation(englishInformation);
                    }
                }
                else
                {
                   // Implement other version (2.0)?
                    serviceModel = null;
                }

                return serviceModel;
            }
            else
                return null;
        }
    }
}