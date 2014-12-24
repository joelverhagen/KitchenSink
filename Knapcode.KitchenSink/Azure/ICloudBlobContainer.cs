using System.Threading;
using System.Threading.Tasks;

namespace Knapcode.KitchenSink.Azure
{
    public interface ICloudBlobContainer
    {
        Task CreateAsync(CancellationToken cancellationToken);
        Task<bool> CreateIfNotExistsAsync(CancellationToken cancellationToken);
        Task DeleteAsync(CancellationToken cancellationToken);
        Task<bool> DeleteIfExistsAsync(CancellationToken cancellationToken);
        Task<bool> ExistsAsync(CancellationToken cancellationToken);
        ICloudBlockBlob GetBlockBlobReference(string blobName);
    }
}