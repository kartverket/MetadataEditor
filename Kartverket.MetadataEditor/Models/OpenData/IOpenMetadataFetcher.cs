using System.Threading.Tasks;

namespace Kartverket.MetadataEditor.Models.OpenData
{
    public interface IOpenMetadataFetcher
    {
        Task<Metadata> FetchMetadataAsync(OpenMetadataEndpoint endpoint);
    }
}
