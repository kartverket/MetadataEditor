using Kartverket.MetadataEditor.Models;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kartverket.MetadataEditor.Tests.Models
{
    public class WfsGetCapabilities11ParserTest
    {
        private string xmlFile;

        [Fact]
        public void ShouldParseFeatureTypesFromWfs1_1_GetCapabilitiesDocument()
        {
            xmlFile = File.ReadAllText("xml\\WFS_1_1_GetCapabilitiesWithFeatureTypes.xml");
            XDocument doc = XDocument.Parse(xmlFile);
            WfsServiceViewModel serviceModel = new WfsGetCapabilities11Parser().Parse(doc);

            Assert.NotNull(serviceModel.Layers);
            Assert.Equal(5, serviceModel.Layers.Count);
        }
    }
}
