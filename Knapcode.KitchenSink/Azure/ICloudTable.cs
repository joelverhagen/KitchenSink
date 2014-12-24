using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Knapcode.KitchenSink.Azure
{
    public interface ICloudTable
    {
        Task<TableResult> ExecuteAsync(TableOperation operation, CancellationToken cancellationToken);
        Task<TableQuerySegment<TResult>> ExecuteQuerySegmentedAsync<TResult>(TableQuery query, EntityResolver<TResult> resolver, TableContinuationToken token, CancellationToken cancellationToken);
        Task<TableQuerySegment<TElement>> ExecuteQuerySegmentedAsync<TElement>(TableQuery<TElement> query, TableContinuationToken token, CancellationToken cancellationToken) where TElement : ITableEntity, new();
        Task CreateAsync(CancellationToken cancellationToken);
        Task<bool> CreateIfNotExistsAsync(CancellationToken cancellationToken);
        Task DeleteAsync(CancellationToken cancellationToken);
        Task<bool> DeleteIfExistsAsync(CancellationToken cancellationToken);
        Task<bool> ExistsAsync(CancellationToken cancellationToken);
    }
}