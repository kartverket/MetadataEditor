﻿using System.Collections.Generic;
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
        string CopyMetadata(string uuid, string username);
        void SaveMetadataModel(MetadataViewModel model, string username);
        List<WfsLayerViewModel> CreateMetadataForFeature(string uuid, List<WfsLayerViewModel> createMetadataForLayers, string[] keywords, string username);
        List<WmsLayerViewModel> CreateMetadataForLayers(string uuid, List<WmsLayerViewModel> createMetadataForLayers, string[] keywords, string username);
        void DeleteMetadata(MetadataViewModel model, string user, string comment);
        Dictionary<DistributionGroup, Distribution> GetFormatDistributions(List<SimpleDistribution> distributionsFormats);
        Stream SaveMetadataAsXml(MetadataViewModel model);
        Task<List<LogEntry>> GetLogEntries(string uuid);
        Task<List<LogEntry>> GetLogEntriesLatest(int limitNumberOfEntries = 50, string operation = "");

        Dictionary<string, string> GetPriorityDatasets();
        Dictionary<string, string> GetSpatialScopes();
        void UpdateRegisterTranslations(string username, string uuid);
        Dictionary<string, string> GetInspireAccessRestrictions(string culture);
    }
}