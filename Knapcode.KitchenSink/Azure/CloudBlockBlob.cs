using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Knapcode.KitchenSink.Azure
{
    public class CloudBlockBlob : ICloudBlockBlob
    {
        private readonly Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob _blockBlob;

        public CloudBlockBlob(Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob blockBlob)
        {
            _blockBlob = blockBlob;
        }

        public Task UploadFromStreamAsync(Stream source, CancellationToken cancellationToken)
        {
            return _blockBlob.UploadFromStreamAsync(source, cancellationToken);
        }

        public Task DownloadToStreamAsync(Stream target, CancellationToken cancellationToken)
        {
            return _blockBlob.DownloadToStreamAsync(target, cancellationToken);
        }

        public Task<bool> ExistsAsync(BlobRequestOptions options, OperationContext operationContext)
        {
            return _blockBlob.ExistsAsync(options, operationContext);
        }

        public Task DeleteAsync(CancellationToken cancellationToken)
        {
            return _blockBlob.DeleteAsync(cancellationToken);
        }

        public Task<bool> DeleteIfExistsAsync(CancellationToken cancellationToken)
        {
            return _blockBlob.DeleteIfExistsAsync(cancellationToken);
        }

        public Task PutBlockAsync(string blockId, Stream blockData, string contentMD5, CancellationToken cancellationToken)
        {
            return _blockBlob.PutBlockAsync(blockId, blockData, contentMD5, cancellationToken);
        }

        public Task PutBlockListAsync(IEnumerable<string> blockList, CancellationToken cancellationToken)
        {
            return _blockBlob.PutBlockListAsync(blockList, cancellationToken);
        }

        public Task<CloudBlobStream> OpenWriteAsync(CancellationToken cancellationToken)
        {
            return _blockBlob.OpenWriteAsync(cancellationToken);
        }

        public Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            return _blockBlob.OpenReadAsync(cancellationToken);
        }
    }
}