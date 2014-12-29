using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Knapcode.KitchenSink.Azure;
using Knapcode.KitchenSink.Tests.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Knapcode.KitchenSink.Tests.Azure
{
    [TestClass]
    public class DelegatedCloudTableTests
    {
        [TestMethod, TestCategory("Integration")]
        public async Task ExecuteAsync_WithInsert_Succeeds()
        {
            // ARRANGE
            var ts = new TestState();
            
            // ACT
            var result = await ts.Table.ExecuteAsync(TableOperation.Insert(ts.EntityA), CancellationToken.None);

            // ASSERT
            result.HttpStatusCode.Should().BeGreaterOrEqualTo(200).And.BeLessThan(300);
            result.Etag.Should().NotBeNull();
        }

        [TestMethod, TestCategory("Integration")]
        public async Task ExecuteAsync_WithConflictingInsert_ThrowsException()
        {
            // ARRANGE
            var ts = new TestState();
            await ts.Table.ExecuteAsync(TableOperation.Insert(ts.EntityA), CancellationToken.None);
            ts.EntityA.ETag = null;

            // ACT
            Func<Task> action = () => ts.Table.ExecuteAsync(TableOperation.Insert(ts.EntityA), CancellationToken.None);
            
            // ASSERT
            StorageException exception = action.ShouldThrow<StorageException>().And;
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<WebException>();
            var innerException = ((WebException) exception.InnerException);
            innerException.Response.Should().BeOfType<HttpWebResponse>();
            var response = ((HttpWebResponse) innerException.Response);
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [TestMethod, TestCategory("Integration")]
        public async Task ExecuteAsync_WithConflictingReplace_ThrowsException()
        {
            // ARRANGE
            var ts = new TestState();
            var result = await ts.Table.ExecuteAsync(TableOperation.Insert(ts.EntityA), CancellationToken.None);
            await ts.Table.ExecuteAsync(TableOperation.Replace(ts.EntityA), CancellationToken.None);
            ts.EntityA.ETag = result.Etag;

            // ACT
            Func<Task> action = () => ts.Table.ExecuteAsync(TableOperation.Replace(ts.EntityA), CancellationToken.None);

            // ASSERT
            StorageException exception = action.ShouldThrow<StorageException>().And;
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<WebException>();
            var innerException = ((WebException)exception.InnerException);
            innerException.Response.Should().BeOfType<HttpWebResponse>();
            var response = ((HttpWebResponse)innerException.Response);
            response.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        }

        [TestMethod, TestCategory("Integration")]
        public async Task ExecuteAsync_WithValidBatch_Succeeds()
        {
            // ARRANGE
            var ts = new TestState();
            var batch = new TableBatchOperation
            {
                TableOperation.Insert(ts.EntityA),
                TableOperation.Insert(ts.EntityB)
            };

            // ACT
            await ts.Table.ExecuteBatchAsync(batch, CancellationToken.None);

            // ASSERT
            await ts.VerifyContentAsync(ts.EntityA.RowKey, ts.EntityA.Content);
            await ts.VerifyContentAsync(ts.EntityB.RowKey, ts.EntityB.Content);
        }


        [TestMethod, TestCategory("Integration")]
        public async Task ExecuteAsync_WithInvalidBatch_RollsBack()
        {
            // ARRANGE
            var ts = new TestState();
            ts.EntityA.Content = ts.ContentA;
            await ts.Table.ExecuteAsync(TableOperation.Insert(ts.EntityA), CancellationToken.None);
            ts.EntityA.ETag = null;
            ts.EntityA.Content = ts.ContentB;

            var batch = new TableBatchOperation
            {
                TableOperation.Insert(ts.EntityB),
                TableOperation.Insert(ts.EntityA)
            };

            // ACT
            Func<Task> action = () => ts.Table.ExecuteBatchAsync(batch, CancellationToken.None);

            // ASSERT
            action.ShouldThrow<StorageException>();
            await ts.VerifyDoesNotExistAsync(ts.EntityB.RowKey);
            await ts.VerifyContentAsync(ts.EntityA.RowKey, ts.ContentA);
        }

        private class TestState
        {
            public TestState()
            {
                CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;
                var tableClient = account.CreateCloudTableClient();

                Table = new DelegatingCloudTable(tableClient.GetTableReference(Constants.TestTableName));
                Table.CreateIfNotExistsAsync(CancellationToken.None).Wait();

                PartitionKey = Guid.NewGuid().ToString();
                RowKeyA = Guid.NewGuid().ToString();
                RowKeyB = Guid.NewGuid().ToString();
                ContentA = "A";
                ContentB = "B";
                EntityA = new JsonSerializedTableEntity<string> {PartitionKey = PartitionKey, RowKey = RowKeyA, Content = ContentA};
                EntityB = new JsonSerializedTableEntity<string> { PartitionKey = PartitionKey, RowKey = RowKeyB, Content = ContentA };
            }

            public DelegatingCloudTable Table { get; set; }

            public string PartitionKey { get; set; }

            public string RowKeyA { get; set; }

            public string RowKeyB { get; set; }

            public string ContentA { get; set; }

            public string ContentB { get; set; }

            public JsonSerializedTableEntity<string> EntityA { get; set; }

            public JsonSerializedTableEntity<string> EntityB { get; set; }

            public async Task VerifyContentAsync(string rowKey, string content)
            {
                var result = await Table.ExecuteAsync(TableOperation.Retrieve<JsonSerializedTableEntity<string>>(this.PartitionKey, rowKey), CancellationToken.None);
                result.HttpStatusCode.Should().Be((int) HttpStatusCode.OK);
                result.Result.Should().BeOfType<JsonSerializedTableEntity<string>>();
                ((JsonSerializedTableEntity<string>) result.Result).Content.Should().Be(content);
            }

            public async Task VerifyDoesNotExistAsync(string rowKey)
            {
                var result = await Table.ExecuteAsync(TableOperation.Retrieve<JsonSerializedTableEntity<string>>(this.PartitionKey, rowKey), CancellationToken.None);
                result.HttpStatusCode.Should().Be((int)HttpStatusCode.NotFound);
            }
        }
    }
}
