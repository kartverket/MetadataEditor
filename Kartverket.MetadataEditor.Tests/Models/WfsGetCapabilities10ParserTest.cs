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
    public class WfsGetCapabilities10ParserTest
    {
        private string xmlFile;


        [Fact]
        public void ShouldParseFeatureTypesFromWfs1_0_GetCapabilitiesDocument()
        {
            xmlFile = File.ReadAllText("xml\\WFS_1_0_GetCapabilitiesWithFeatureTypes.xml");
            XDocument doc = XDocument.Parse(xmlFile);
            WfsServiceViewModel serviceModel = new WfsGetCapabilities10Parser().Parse(doc);

            Assert.NotNull(serviceModel.Layers);
            Assert.Equal(12, serviceModel.Layers.Count);
        }
    }
}
