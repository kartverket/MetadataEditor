using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kartverket.MetadataEditor.Models.OpenData
{
    public interface IOpenMetadataService
    {
        Task<int> SynchronizeMetadata(List<OpenMetadataEndpoint> metadataEndpoints, string username);
    }
}
