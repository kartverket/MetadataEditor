using System.Collections.Generic;
using System.IO;
using GeoNorgeAPI;

namespace Kartverket.MetadataEditor.Models
{
    public interface IMetadataService
    {
        MetadataViewModel GetMetadataModel(string uuid);
        MetadataIndexViewModel SearchMetadata(string v1, string v2, int next, int limit);
        string CreateMetadata(MetadataCreateViewModel newMetadata, string username);
        void SaveMetadataModel(MetadataViewModel model, string username);
        List<WfsLayerViewModel> CreateMetadataForFeature(string uuid, List<WfsLayerViewModel> createMetadataForLayers, string[] keywords, string username);
        List<WmsLayerViewModel> CreateMetadataForLayers(string uuid, List<WmsLayerViewModel> createMetadataForLayers, string[] keywords, string username);
        Dictionary<string, string> CreateAdditionalHeadersWithUsername(string username, string published="");
        void DeleteMetadata(string uuid, string v);
        Dictionary<DistributionGroup, Distribution> GetFormatDistributions(List<SimpleDistribution> distributionsFormats);
        Stream SaveMetadataAsXml(MetadataViewModel model);
    }
}