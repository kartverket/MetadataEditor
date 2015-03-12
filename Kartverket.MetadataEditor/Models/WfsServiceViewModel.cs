using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class WfsServiceViewModel
    {
        public bool SupportsEnglishGetCapabilities { get; set; }
        public List<WfsLayerViewModel> Layers { get; set; }


        public WfsServiceViewModel()
        {
            Layers = new List<WfsLayerViewModel>();
        }

        internal void AppendEnglishInformation(Dictionary<string, Dictionary<string, string>> englishInfo)
        {
            if (Layers != null && englishInfo != null)
            {
                foreach (var layer in Layers)
                {
                    if (englishInfo.Count > 0 && englishInfo.ContainsKey(layer.Name))
                    {
                       Dictionary<string, string> englishTexts = englishInfo[layer.Name];
                       if (englishTexts != null)
                       {
                           layer.EnglishTitle = englishTexts["title"];
                           layer.EnglishAbstract = englishTexts["abstract"];
                       }
                    }
                }
            }
        }
    }
}