using System.Threading.Tasks;

namespace Kartverket.MetadataEditor.Models.OpenData
{
    public interface IOpenMetadataService
    {
        Task<int> SynchronizeMetadata();
    }
}
