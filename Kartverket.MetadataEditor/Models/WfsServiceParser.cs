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
        private static XNamespace WFS2 = "http://www.opengis.net/wfs/2.0";
        private static XNamespace INSPIRE = "http://inspire.ec.europa.eu/schemas/inspire_vs/1.0";
        private static XNamespace INSPIRE_COMMON = "http://inspire.ec.europa.eu/schemas/common/1.0";

        public WfsServiceViewModel GetLayers(string wfsUrl)
        {
            if (!string.IsNullOrWhiteSpace(wfsUrl))
            {
                WfsServiceViewModel serviceModel = new WfsServiceViewModel();

                wfsUrl = RemoveQueryString(wfsUrl); 
                wfsUrl = wfsUrl + "?service=wfs&request=GetCapabilities";

                XDocument xmlDocument = XDocument.Load(wfsUrl);

                XElement root = xmlDocument.Element(WFS + "WFS_Capabilities");
                if (root != null)
                {
                    string version = root.Attribute("version").Value;
                    version = version.Substring(0, 3);

                    if (version == "1.0")
                    {
                    WfsGetCapabilities10Parser parser = new WfsGetCapabilities10Parser();
                    serviceModel = parser.Parse(xmlDocument);
                    }

                    else if (version == "1.1")
                    {
                    WfsGetCapabilities11Parser parser = new WfsGetCapabilities11Parser();
                    serviceModel = parser.Parse(xmlDocument);
                    }
                    else
                    {
                        WfsGetCapabilities11Parser parser = new WfsGetCapabilities11Parser();
                        serviceModel = parser.Parse(xmlDocument);
                    }

                }
                else
                {
                    XElement root2 = xmlDocument.Element(WFS2 + "WFS_Capabilities");
                    if (root2 != null)
                    {
                        WfsGetCapabilities20Parser parser = new WfsGetCapabilities20Parser();
                        serviceModel = parser.Parse(xmlDocument);
                    }
                    else
                        serviceModel = null;
                }

                return serviceModel;
            }
            else
                return null;
        }

        string RemoveQueryString(string URL)
        {
            int startQueryString = URL.IndexOf("?");

            if (startQueryString != -1)
                URL = URL.Substring(0, startQueryString);

            return URL;
        }
    }
}