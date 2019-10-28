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
    public class WmsGetCapabilities11ParserTest
    {

        private string xmlFile;

        [Fact]
        public void ShouldParseLayerGroupsFromWms1_1_GetCapabilitiesDocument()
        {
            xmlFile = File.ReadAllText("xml\\WMS_1_1_GetCapabilitiesWithLayerGroups.xml");
            XDocument doc = XDocument.Parse(xmlFile);
            WmsServiceViewModel serviceModel = new WmsGetCapabilities11Parser().Parse(doc);

            Assert.NotNull(serviceModel.Layers);
            Assert.Equal(26, serviceModel.Layers.Count);
        }

    }
}
