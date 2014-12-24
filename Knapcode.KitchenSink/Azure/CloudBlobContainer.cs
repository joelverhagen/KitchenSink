using System.Threading;
using System.Threading.Tasks;

namespace Knapcode.KitchenSink.Azure
{
    public class CloudBlobContainer : ICloudBlobContainer
    {
        private readonly Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer _blobContainer;

        public CloudBlobContainer(Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer blobContainer)
        {
            _blobContainer = blobContainer;
        }

        public Task CreateAsync(CancellationToken cancellationToken)
        {
            return _blobContainer.CreateAsync(cancellationToken);
        }

        public Task<bool> CreateIfNotExistsAsync(CancellationToken cancellationToken)
        {
            return _blobContainer.CreateIfNotExistsAsync(cancellationToken);
        }

        public Task DeleteAsync(CancellationToken cancellationToken)
        {
            return _blobContainer.DeleteAsync(cancellationToken);
        }

        public Task<bool> DeleteIfExistsAsync(CancellationToken cancellationToken)
        {
            return _blobContainer.DeleteIfExistsAsync(cancellationToken);
        }

        public Task<bool> ExistsAsync(CancellationToken cancellationToken)
        {
            return _blobContainer.ExistsAsync(cancellationToken);
        }

        public ICloudBlockBlob GetBlockBlobReference(string blobName)
        {
            return new CloudBlockBlob(_blobContainer.GetBlockBlobReference(blobName));
        }
    }
}
