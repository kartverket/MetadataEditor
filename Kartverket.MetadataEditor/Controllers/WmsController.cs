using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Kartverket.MetadataEditor.Controllers
{
    public class WmsController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> GetLayers(string serviceUrl)
        {
            if (string.IsNullOrWhiteSpace(serviceUrl))
                return Json(new { layers = new object[0] }, JsonRequestBehavior.AllowGet);

            try
            {
                using (var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true }))
                {
                    client.Timeout = TimeSpan.FromSeconds(120);
                    var resp = await client.GetAsync(serviceUrl);
                    resp.EnsureSuccessStatusCode();

                    var xml = await resp.Content.ReadAsStringAsync();
                    var layers = ParseWmsLayers(xml)
                        .Select(l => new { name = l.Name, title = l.Title })
                        .ToArray();

                    return Json(new { layers }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new { layers = new object[0] }, JsonRequestBehavior.AllowGet);
            }
        }

        private static IEnumerable<WmsLayer> ParseWmsLayers(string xml)
        {
            var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
            // Ignore namespaces by using LocalName
            var layerElems = doc.Descendants().Where(e => e.Name.LocalName == "Layer");
            var result = new List<WmsLayer>();

            foreach (var layer in layerElems)
            {
                var nameElem = layer.Elements().FirstOrDefault(e => e.Name.LocalName == "Name");
                var titleElem = layer.Elements().FirstOrDefault(e => e.Name.LocalName == "Title");

                // Only include named layers
                if (nameElem != null)
                {
                    result.Add(new WmsLayer
                    {
                        Name = nameElem.Value?.Trim(),
                        Title = titleElem?.Value?.Trim()
                    });
                }
            }

            // Deduplicate by Name
            return result
                .Where(l => !string.IsNullOrEmpty(l.Name))
                .GroupBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First());
        }

        private class WmsLayer
        {
            public string Name { get; set; }
            public string Title { get; set; }
        }
    }
}