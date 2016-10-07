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
    class WfsGetCapabilities20ParserTest
    {
        private string xmlFile;
        [SetUp]
        public void SetUp()
        {
            xmlFile = File.ReadAllText("xml\\WFS_2_0_GetCapabilitiesWithFeatureTypes.xml");
        }


        [Test]
        public void ShouldParseFeatureTypesFromWfs2_0_GetCapabilitiesDocument()
        {
            XDocument doc = XDocument.Parse(xmlFile);
            WfsServiceViewModel serviceModel = new WfsGetCapabilities20Parser().Parse(doc);

            Assert.NotNull(serviceModel.Layers, "No features/layers found");
            Assert.AreEqual(1, serviceModel.Layers.Count, "Should have featureType");
        }
    }
}
