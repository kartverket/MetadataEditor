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
    class WfsGetCapabilities11ParserTest
    {
        private string xmlFile;
        [SetUp]
        public void SetUp()
        {
            xmlFile = File.ReadAllText("xml\\WFS_1_1_GetCapabilitiesWithFeatureTypes.xml");
        }


        [Test]
        public void ShouldParseLayerGroupsFromWfs1_1_GetCapabilitiesDocument()
        {
            XDocument doc = XDocument.Parse(xmlFile);
            WfsServiceViewModel serviceModel = new WfsGetCapabilities11Parser().Parse(doc);

            Assert.NotNull(serviceModel.Layers, "No layers found");
            Assert.AreEqual(5, serviceModel.Layers.Count, "Should have many featureTypes");
        }
    }
}
