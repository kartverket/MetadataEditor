using Kartverket.MetadataEditor.Models;
using NUnit.Framework;
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
    [TestFixture]
    class WmsGetCapabilities11ParserTest
    {

        private string xmlFile;
        [SetUp]
        public void SetUp()
        {
            xmlFile = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\xml\\WMS_1_1_GetCapabilitiesWithLayerGroups.xml");
        }

        [Test]
        public void ShouldParseLayerGroupsFromWms1_1_GetCapabilitiesDocument()
        {
            XDocument doc = XDocument.Parse(xmlFile);
            WmsServiceViewModel serviceModel = new WmsGetCapabilities11Parser().Parse(doc);

            Assert.NotNull(serviceModel.Layers, "No layers found");
            Assert.AreEqual(26, serviceModel.Layers.Count, "Should have many layers");
        }

    }
}
