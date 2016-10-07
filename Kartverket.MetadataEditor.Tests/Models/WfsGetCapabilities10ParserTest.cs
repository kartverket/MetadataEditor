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
    class WfsGetCapabilities10ParserTest
    {
        private string xmlFile;
        [SetUp]
        public void SetUp()
        {
            xmlFile = File.ReadAllText("xml\\WFS_1_0_GetCapabilitiesWithFeatureTypes.xml");
        }


        [Test]
        public void ShouldParseFeatureTypesFromWfs1_0_GetCapabilitiesDocument()
        {
            XDocument doc = XDocument.Parse(xmlFile);
            WfsServiceViewModel serviceModel = new WfsGetCapabilities10Parser().Parse(doc);

            Assert.NotNull(serviceModel.Layers, "No features/layers found");
            Assert.AreEqual(12, serviceModel.Layers.Count, "Should have many featureTypes");
        }
    }
}
