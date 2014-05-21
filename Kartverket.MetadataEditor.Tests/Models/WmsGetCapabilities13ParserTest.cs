using Kartverket.MetadataEditor.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kartverket.MetadataEditor.Tests.Models
{
    [TestFixture]
    public class WmsGetCapabilities13ParserTest
    {
        private string xmlFile;
        [SetUp]
        public void SetUp()
        {
            xmlFile = File.ReadAllText("xml\\WMS_1_3_GetCapabiltiesWithLayerGroups.xml");
        }


        [Test]
        public void ShouldParseLayerGroupsFromWms1_3_GetCapabilitiesDocument()
        {
            XDocument doc = XDocument.Parse(xmlFile);
            WmsServiceViewModel serviceModel = new WmsGetCapabilities13Parser().Parse(doc);

            Assert.NotNull(serviceModel.Layers, "No layers found");
            Assert.AreEqual(26, serviceModel.Layers.Count, "Should have many layers");
        }
    }
}
