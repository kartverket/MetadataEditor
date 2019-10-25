using System;
using System.Data.Entity;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Helpers;
using Arkitektum.GIS.Lib.SerializeUtil;
using FluentAssertions;
using GeoNorgeAPI;
using Kartverket.MetadataEditor.Models;
using Kartverket.MetadataEditor.Models.OpenData;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Kartverket.MetadataEditor.Tests.Models.OpenData
{
    public class OpenMetadataServiceTest
    {
        private readonly OpenMetadataEndpoint _endpoint = new OpenMetadataEndpoint() { OrganizationName = "tromsø", Url=""};

        private static readonly MetadataViewModel MetadataViewModel = new MetadataViewModel()
        {
            ContactMetadata = new Contact(),
            ContactOwner = new Contact(),
            ContactPublisher = new Contact()
        };

        [Test]
        public async Task CreateNewMetadataWhenItDoesNotExistInGeonorge()
        {
            var metadataFetcherMock = new Mock<IOpenMetadataFetcher>();

            Metadata metadata = SetupMetadataFetcherMock(metadataFetcherMock, _endpoint);

            var metadataServiceMock = SetupMetadataServiceMock(metadata);

            var geoNorgeMock = new Mock<IGeoNorge>();

            var openMetadataService = new OpenMetadataService(metadataServiceMock.Object, metadataFetcherMock.Object, geoNorgeMock.Object);
            
            var numberOfUpdatedDatasets = await openMetadataService.SynchronizeMetadata(_endpoint);

            metadataServiceMock
                .Verify(m => m.CreateMetadata(It.IsAny<MetadataCreateViewModel>(), It.IsAny<string>()), Times.Exactly(3));

            metadataServiceMock
                .Verify(m => m.SaveMetadataModel(It.IsAny<MetadataViewModel>(), It.IsAny<string>()), Times.Exactly(16));

            numberOfUpdatedDatasets.Should().Be(16);
        }

        [Test]
        public async Task ShouldUpdateMetadataWhenItAlreadyExistsInGeonorge()
        {
            var metadataFetcherMock = new Mock<IOpenMetadataFetcher>();

            Metadata metadata = SetupMetadataFetcherMock(metadataFetcherMock, _endpoint);

            var metadataServiceMock = SetupMetadataServiceMockToReturnModel(metadata);

            var geoNorgeMock = new Mock<IGeoNorge>();

            var openMetadataService = new OpenMetadataService(metadataServiceMock.Object, metadataFetcherMock.Object, geoNorgeMock.Object);
            
            var numberOfUpdatedDatasets = await openMetadataService.SynchronizeMetadata(_endpoint);

            metadataServiceMock
                .Verify(m => m.CreateMetadata(It.IsAny<MetadataCreateViewModel>(), It.IsAny<string>()), Times.Exactly(3));

            metadataServiceMock
                .Verify(m => m.SaveMetadataModel(It.IsAny<MetadataViewModel>(), It.IsAny<string>()), Times.Exactly(16));

            numberOfUpdatedDatasets.Should().Be(16);
        }



        private static Metadata SetupMetadataFetcherMock(Mock<IOpenMetadataFetcher> metadataFetcherMock, OpenMetadataEndpoint endpoint)
        {
            Metadata metadata = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Models\\OpenData\\data.json"));

            metadataFetcherMock.Setup(f => f.FetchMetadataAsync(endpoint)).ReturnsAsync(metadata);
            return metadata;
        }

        /// <summary>
        /// Setups mock to return null the first time we retrieve metadata and an object the second time
        /// </summary>
        /// <param name="metadata"></param>
        private static Mock<IMetadataService> SetupMetadataServiceMock(Metadata metadata)
        {
            var metadataServiceMock = new Mock<IMetadataService>();

            foreach (var dataset in metadata.dataset)
            {
                metadataServiceMock.SetupSequence(m =>
                        m.GetMetadataModel(OpenMetadataService.GetIdentifierFromUri(dataset.identifier)))
                    //.Returns(null)
                    .Returns(MetadataViewModel);
            }

            return metadataServiceMock;
        }

        /// <summary>
        /// Setups mock to return model when dataset identifiers are given as input
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        private static Mock<IMetadataService> SetupMetadataServiceMockToReturnModel(Metadata metadata)
        {
            var metadataServiceMock = new Mock<IMetadataService>();

            foreach (var dataset in metadata.dataset)
            {
                metadataServiceMock.Setup(m =>
                        m.GetMetadataModel(OpenMetadataService.GetIdentifierFromUri(dataset.identifier)))
                    .Returns(MetadataViewModel);
            }

            return metadataServiceMock;
        }
    }

}
