using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Knapcode.KitchenSink.Azure
{
    public interface ICloudQueue
    {
        Task CreateAsync(CancellationToken cancellationToken);
        Task<bool> CreateIfNotExistsAsync(CancellationToken cancellationToken);
        Task<bool> DeleteIfExistsAsync(CancellationToken cancellationToken);
        Task DeleteAsync(CancellationToken cancellationToken);
        Task<bool> ExistsAsync(CancellationToken cancellationToken);
        Task UpdateMessageAsync(CloudQueueMessage message, TimeSpan visibilityTimeout, MessageUpdateFields updateFields, CancellationToken cancellationToken);
        Task AddMessageAsync(CloudQueueMessage message, CancellationToken cancellationToken);
        Task DeleteMessageAsync(CloudQueueMessage message, CancellationToken cancellationToken);
        Task DeleteMessageAsync(string messageId, string popReceipt, CancellationToken cancellationToken);
        Task ClearAsync(CancellationToken cancellationToken);
        Task<CloudQueueMessage> GetMessageAsync(CancellationToken cancellationToken);
        Task<IEnumerable<CloudQueueMessage>> GetMessagesAsync(int messageCount, CancellationToken cancellationToken);
        Task<IEnumerable<CloudQueueMessage>> PeekMessagesAsync(int messageCount, CancellationToken cancellationToken);
    }
}