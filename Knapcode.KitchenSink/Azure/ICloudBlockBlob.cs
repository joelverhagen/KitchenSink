using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Knapcode.KitchenSink.Azure
{
    public interface ICloudBlockBlob
    {
        Task UploadFromStreamAsync(Stream source, CancellationToken cancellationToken);
        Task DownloadToStreamAsync(Stream target, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(BlobRequestOptions options, OperationContext operationContext);
        Task DeleteAsync(CancellationToken cancellationToken);
        Task<bool> DeleteIfExistsAsync(CancellationToken cancellationToken);
        Task PutBlockAsync(string blockId, Stream blockData, string contentMD5, CancellationToken cancellationToken);
        Task PutBlockListAsync(IEnumerable<string> blockList, CancellationToken cancellationToken);
        Task<CloudBlobStream> OpenWriteAsync(CancellationToken cancellationToken);
        Task<Stream> OpenReadAsync(CancellationToken cancellationToken);
    }
}