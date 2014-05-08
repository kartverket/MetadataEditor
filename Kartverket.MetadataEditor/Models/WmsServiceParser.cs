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

        public List<WmsLayerViewModel> GetLayers(string wmsUrl)
        {
            if (!wmsUrl.EndsWith("?"))
            {
                wmsUrl = wmsUrl + "?";
            }
            wmsUrl = wmsUrl + "service=wms&request=GetCapabilities";

            XDocument xd = XDocument.Load(wmsUrl);

            IEnumerable<XElement> layers =
                    from el in xd.Elements(WMS + "WMS_Capabilities").Elements(WMS + "Capability").Elements(WMS + "Layer").Elements(WMS + "Layer")
                    select el;

            Dictionary<string, Dictionary<string, string>> englishData = new Dictionary<string, Dictionary<string, string>>();
            XElement extendedCapabilities = xd.Element(WMS + "WMS_Capabilities").Element(WMS + "Capability").Element(INSPIRE + "ExtendedCapabilities");
            if (extendedCapabilities != null)
            {
                XElement supportedLanguages = extendedCapabilities.Element(INSPIRE_COMMON + "SupportedLanguages");
                if (supportedLanguages != null)
                {
                    XElement language = supportedLanguages.Element(INSPIRE_COMMON + "SupportedLanguage").Element(INSPIRE_COMMON + "Language");
                    if (language.Value == "eng")
                    {
                        XDocument getCapabilitiesEnglish = XDocument.Load(wmsUrl + "&language=eng");
                        IEnumerable<XElement> englishLayers =
                            from el in getCapabilitiesEnglish.Elements(WMS + "WMS_Capabilities").Elements(WMS + "Capability").Elements(WMS + "Layer").Elements(WMS + "Layer")
                            select el;

                        if (englishLayers != null)
                        {
                            foreach (var layer in englishLayers)
                            {
                                var nameElement = layer.Element(WMS + "Name");
                                string name = nameElement != null ? nameElement.Value : null;
                                var titleElement = layer.Element(WMS + "Title");
                                string title = titleElement != null ? titleElement.Value : null;
                                var abstractElement = layer.Element(WMS + "Abstract");
                                string @abstract = abstractElement != null ? abstractElement.Value : null;

                                var values = new Dictionary<string, string>();
                                values.Add("title", title);
                                values.Add("abstract", @abstract);

                                englishData.Add(name, values);
                            }
                        }
                    }
                }
            }


            List<WmsLayerViewModel> models = new List<WmsLayerViewModel>();

            foreach (var layer in layers)
            {
                var boundingBox = layer.Element(WMS + "EX_GeographicBoundingBox");
                var nameElement = layer.Element(WMS +"Name");
                var titleElement = layer.Element(WMS + "Title");
                var abstractElement = layer.Element(WMS + "Abstract");
                List<string> keywords = ParseKeywords(layer.Element(WMS + "KeywordList"));

                string name = nameElement != null ? nameElement.Value : null;
                string englishTitle = null;
                string englishAbstract = null;
                if (englishData.Count > 0 && englishData.ContainsKey(name)) {
                    Dictionary<string, string> englishTexts = englishData[name];
                    if (englishTexts != null)
                    {
                        englishTitle = englishTexts["title"];
                        englishAbstract = englishTexts["abstract"];
                    }
                }
                models.Add(new WmsLayerViewModel
                {
                    Name = name,
                    Title = titleElement != null ? titleElement.Value : null,
                    Abstract = abstractElement != null ? abstractElement.Value : null,
                    BoundingBoxEast = boundingBox != null ? boundingBox.Element(WMS + "eastBoundLongitude").Value : null,
                    BoundingBoxWest = boundingBox != null ? boundingBox.Element(WMS + "westBoundLongitude").Value : null,
                    BoundingBoxNorth = boundingBox != null ? boundingBox.Element(WMS + "northBoundLatitude").Value : null,
                    BoundingBoxSouth = boundingBox != null ? boundingBox.Element(WMS + "southBoundLatitude").Value : null,
                    Keywords = keywords,
                    EnglishTitle = englishTitle,
                    EnglishAbstract = englishAbstract
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