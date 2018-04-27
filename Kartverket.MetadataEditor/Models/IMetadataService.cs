using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities.LogEntry;

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
        void DeleteMetadata(MetadataViewModel model, string user);
        Dictionary<DistributionGroup, Distribution> GetFormatDistributions(List<SimpleDistribution> distributionsFormats);
        Stream SaveMetadataAsXml(MetadataViewModel model);
        Task<List<LogEntry>> GetLogEntries(string uuid);
        Task<List<LogEntry>> GetLogEntriesLatest(int limitNumberOfEntries = 50, string operation = "");

        Dictionary<string, string> GetPriorityDatasets();
        void UpdateRegisterTranslations(string v, string uuid);
    }
}