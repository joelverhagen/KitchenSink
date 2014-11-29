using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Knapcode.KitchenSink.Http.Handlers;
using Knapcode.KitchenSink.Http.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace Knapcode.KitchenSink.Tests.Http.Handlers
{
    [TestClass]
    public class AzureHttpMessageStoreTests
    {
        [TestMethod, TestCategory("Integration")]
        public async Task StoreRequestAsync_WithContent_KeepsEquivalentContent()
        {
            // ARRANGE
            var ts = await TestState.InitializeAsync();
            string expectedContent = await ts.RequestWithContent.Content.ReadAsStringAsync();
            KeyValuePair<string, string[]>[] expectedContentHeaders = CopyHeaders(ts.RequestWithContent.Content.Headers);

            // ACT
            await ts.Store.StoreRequestAsync(ts.RequestWithContent, CancellationToken.None);

            // ASSERT
            string actualContent = await ts.RequestWithContent.Content.ReadAsStringAsync();
            actualContent.Should().Be(expectedContent);
            expectedContentHeaders.ShouldBeEquivalentTo(ts.RequestWithContent.Content.Headers);
        }

        [TestMethod, TestCategory("Integration")]
        public async Task StoreRequestAsync_WithAnyRequest_KeepsEquivalentProperties()
        {
            // ARRANGE
            var ts = await TestState.InitializeAsync();
            HttpMethod expectedMethod = ts.RequestWithoutContent.Method;
            Uri expectedRequestUri = new UriBuilder(ts.RequestWithoutContent.RequestUri).Uri;
            var expectedVersion = new Version(ts.RequestWithoutContent.Version.ToString());
            KeyValuePair<string, string[]>[] expectedHeaders = CopyHeaders(ts.RequestWithoutContent.Headers);

            // ACT
            await ts.Store.StoreRequestAsync(ts.RequestWithoutContent, CancellationToken.None);

            // ASSERT
            ts.RequestWithoutContent.Method.Should().Be(expectedMethod);
            ts.RequestWithoutContent.RequestUri.Should().Be(expectedRequestUri);
            ts.RequestWithoutContent.Version.Should().Be(expectedVersion);
            ts.RequestWithoutContent.Headers.ShouldBeEquivalentTo(expectedHeaders);
        }

        [TestMethod, TestCategory("Integration")]
        public async Task StoreRequestAsync_WithAnyRequest_ReturnsAsPopulatedSession()
        {
            // ARRANGE
            var ts = await TestState.InitializeAsync();

            // ACT
            StoredHttpSession session = await ts.Store.StoreRequestAsync(ts.RequestWithoutContent, CancellationToken.None);

            // ASSERT
            session.Should().NotBeNull();
            session.Id.Should().NotBe(default(Guid));
            session.Timestamp.Should().NotBe(default(DateTimeOffset));
        }

        [TestMethod, TestCategory("Integration")]
        public async Task StoreResponseAsync_WithContent_KeepsEquivalentContent()
        {
            // ARRANGE
            var ts = await TestState.InitializeAsync();
            string expectedContent = await ts.ResponseWithContent.Content.ReadAsStringAsync();
            KeyValuePair<string, string[]>[] expectedContentHeaders = CopyHeaders(ts.ResponseWithContent.Content.Headers);
            
            // ACT
            await ts.Store.StoreResponseAsync(ts.Session, ts.ResponseWithContent, CancellationToken.None);

            // ASSERT
            string actualContent = await ts.ResponseWithContent.Content.ReadAsStringAsync();
            actualContent.Should().Be(expectedContent);
            expectedContentHeaders.ShouldBeEquivalentTo(ts.ResponseWithContent.Content.Headers);
        }

        [TestMethod, TestCategory("Integration")]
        public async Task StoreResponseAsync_WithAnyResponse_KeepsEquivalentProperties()
        {
            // ARRANGE
            var ts = await TestState.InitializeAsync();
            var expectedVersion = new Version(ts.ResponseWithoutContent.Version.ToString());
            HttpStatusCode expectedStatusCode = ts.ResponseWithoutContent.StatusCode;
            string expectedReasonPhrase = ts.ResponseWithoutContent.ReasonPhrase;
            KeyValuePair<string, string[]>[] expectedHeaders = CopyHeaders(ts.ResponseWithoutContent.Headers);

            // ACT
            await ts.Store.StoreResponseAsync(ts.Session, ts.ResponseWithoutContent, CancellationToken.None);

            // ASSERT
            ts.ResponseWithoutContent.Version.Should().Be(expectedVersion);
            ts.ResponseWithoutContent.StatusCode.Should().Be(expectedStatusCode);
            ts.ResponseWithoutContent.ReasonPhrase.Should().Be(expectedReasonPhrase);
            ts.ResponseWithoutContent.Headers.ShouldBeEquivalentTo(expectedHeaders);
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetRequestAsync_WithValidSession_ReturnsCorrectRequest()
        {
            // ARRANGE
            var ts = await TestState.InitializeAsync();
            string expectedContent = await ts.RequestWithContent.Content.ReadAsStringAsync();
            KeyValuePair<string, string[]>[] expectedHeaders = CopyHeaders(ts.RequestWithContent.Headers);
            KeyValuePair<string, string[]>[] expectedContentHeaders = CopyHeaders(ts.RequestWithContent.Content.Headers);
            var session = await ts.Store.StoreRequestAsync(ts.RequestWithContent, CancellationToken.None);

            // ACT
            HttpRequestMessage request = await ts.Store.GetRequestAsync(session, CancellationToken.None);

            // ASSERT
            request.Method.Should().Be(ts.RequestWithContent.Method);
            request.RequestUri.Should().Be(ts.RequestWithContent.RequestUri);
            request.Version.Should().Be(ts.RequestWithContent.Version);
            request.Headers.ShouldBeEquivalentTo(expectedHeaders);
            request.Content.Headers.ShouldBeEquivalentTo(expectedContentHeaders);
            var actualContent = await request.Content.ReadAsStringAsync();
            actualContent.Should().Be(expectedContent);
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetRequestAsync_WithInvalidSession_ReturnsNull()
        {
            // ARRANGE
            var ts = await TestState.InitializeAsync();

            // ACT
            HttpRequestMessage request = await ts.Store.GetRequestAsync(ts.Session, CancellationToken.None);

            // ASSERT
            request.Should().BeNull();
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetResponseAsync_WithValidSession_ReturnsCorrectResponse()
        {
            // ARRANGE
            var ts = await TestState.InitializeAsync();
            string expectedContent = await ts.ResponseWithContent.Content.ReadAsStringAsync();
            KeyValuePair<string, string[]>[] expectedHeaders = CopyHeaders(ts.ResponseWithContent.Headers);
            KeyValuePair<string, string[]>[] expectedContentHeaders = CopyHeaders(ts.ResponseWithContent.Content.Headers);
            await ts.Store.StoreResponseAsync(ts.Session, ts.ResponseWithContent, CancellationToken.None);

            // ACT
            HttpResponseMessage response = await ts.Store.GetResponseAsync(ts.Session, CancellationToken.None);

            // ASSERT
            response.Version.Should().Be(ts.ResponseWithContent.Version);
            response.StatusCode.Should().Be(ts.ResponseWithContent.StatusCode);
            response.ReasonPhrase.Should().Be(ts.ResponseWithContent.ReasonPhrase);
            response.Headers.ShouldBeEquivalentTo(expectedHeaders);
            response.Content.Headers.ShouldBeEquivalentTo(expectedContentHeaders);
            var actualContent = await response.Content.ReadAsStringAsync();
            actualContent.Should().Be(expectedContent);
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetResponseAsync_WithInvalidSession_ReturnsNull()
        {
            // ARRANGE
            var ts = await TestState.InitializeAsync();

            // ACT
            HttpResponseMessage response = await ts.Store.GetResponseAsync(ts.Session, CancellationToken.None);

            // ASSERT
            response.Should().BeNull();
        }

        private static KeyValuePair<string, string[]>[] CopyHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            return headers
                .Select(p => new KeyValuePair<string, string[]>(p.Key, p.Value.ToArray()))
                .ToArray();
        }

        private class TestState
        {
            public static async Task<TestState> InitializeAsync()
            {
                CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;

                CloudBlobClient blobClient = account.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference("testcontainer");
                await blobContainer.CreateIfNotExistsAsync();

                CloudTableClient tableClient = account.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("testtable");
                await table.CreateIfNotExistsAsync();

                var requestWithContent = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://www.example.com/"),
                    Version = new Version("1.1"),
                    Content = new StringContent("request content", Encoding.UTF8, "text/plain")
                };
                requestWithContent.Headers.Add("X-Request", "request header");
                requestWithContent.Content.Headers.Add("X-Request-Content", "request content header");

                var requestWithoutContent = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://www.example.com/"),
                    Version = new Version("1.1")
                };
                requestWithoutContent.Headers.Add("X-Request", "request header");

                var responseWithContent = new HttpResponseMessage
                {
                    Version = new Version("1.1"),
                    StatusCode = (HttpStatusCode)429,
                    ReasonPhrase = "Too Many Requests",
                    Content = new StringContent("response content", Encoding.UTF8, "text/plain")
                };
                responseWithContent.Headers.Add("X-Response", "response header");
                responseWithContent.Content.Headers.Add("X-Response-Content", "response content header");

                var responseWithoutContent = new HttpResponseMessage
                {
                    Version = new Version("1.1"),
                    StatusCode = (HttpStatusCode)429,
                    ReasonPhrase = "Too Many Requests"
                };
                responseWithoutContent.Headers.Add("X-Response", "response header");
                
                return new TestState
                {
                    Session = new StoredHttpSession { Id = Guid.NewGuid(), Timestamp = DateTimeOffset.UtcNow },
                    BlobContainer = blobContainer,
                    RequestWithContent = requestWithContent,
                    RequestWithoutContent = requestWithoutContent,
                    ResponseWithContent = responseWithContent,
                    ResponseWithoutContent = responseWithoutContent,
                    Store = new AzureHttpMessageStore(table, blobContainer),
                    Table = table
                };
            }

            public StoredHttpSession Session { get; set; }
            public CloudTable Table { get; set; }
            public CloudBlobContainer BlobContainer { get; set; }
            public AzureHttpMessageStore Store { get; set; }
            public HttpRequestMessage RequestWithContent { get; set; }
            public HttpRequestMessage RequestWithoutContent { get; set; }
            public HttpResponseMessage ResponseWithContent { get; set; }
            public HttpResponseMessage ResponseWithoutContent { get; set; }
        }
    }
}
