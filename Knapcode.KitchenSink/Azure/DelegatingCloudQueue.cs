using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Knapcode.KitchenSink.Azure
{
    public class DelegatingCloudQueue : ICloudQueue
    {
        private readonly CloudQueue _queue;

        public DelegatingCloudQueue(CloudQueue queue)
        {
            _queue = queue;
        }

        public Task CreateAsync(CancellationToken cancellationToken)
        {
            return _queue.CreateAsync(cancellationToken);
        }

        public Task<bool> CreateIfNotExistsAsync(CancellationToken cancellationToken)
        {
            return _queue.CreateIfNotExistsAsync(cancellationToken);
        }

        public Task<bool> DeleteIfExistsAsync(CancellationToken cancellationToken)
        {
            return _queue.DeleteIfExistsAsync(cancellationToken);
        }

        public Task DeleteAsync(CancellationToken cancellationToken)
        {
            return _queue.DeleteAsync(cancellationToken);
        }

        public Task<bool> ExistsAsync(CancellationToken cancellationToken)
        {
            return _queue.ExistsAsync(cancellationToken);
        }

        public Task UpdateMessageAsync(CloudQueueMessage message, TimeSpan visibilityTimeout, MessageUpdateFields updateFields, CancellationToken cancellationToken)
        {
            return _queue.UpdateMessageAsync(message, visibilityTimeout, updateFields, cancellationToken);
        }

        public Task AddMessageAsync(CloudQueueMessage message, CancellationToken cancellationToken)
        {
            return _queue.AddMessageAsync(message, cancellationToken);
        }

        public Task DeleteMessageAsync(CloudQueueMessage message, CancellationToken cancellationToken)
        {
            return _queue.DeleteMessageAsync(message, cancellationToken);
        }

        public Task DeleteMessageAsync(string messageId, string popReceipt, CancellationToken cancellationToken)
        {
            return _queue.DeleteMessageAsync(messageId, popReceipt, cancellationToken);
        }

        public Task ClearAsync(CancellationToken cancellationToken)
        {
            return _queue.ClearAsync(cancellationToken);
        }

        public Task<CloudQueueMessage> GetMessageAsync(CancellationToken cancellationToken)
        {
            return _queue.GetMessageAsync(cancellationToken);
        }

        public Task<IEnumerable<CloudQueueMessage>> GetMessagesAsync(int messageCount, CancellationToken cancellationToken)
        {
            return _queue.GetMessagesAsync(messageCount, cancellationToken);
        }

        public Task<IEnumerable<CloudQueueMessage>> PeekMessagesAsync(int messageCount, CancellationToken cancellationToken)
        {
            return _queue.PeekMessagesAsync(messageCount, cancellationToken);
        }
    }
}
