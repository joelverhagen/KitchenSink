using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Knapcode.KitchenSink.Azure;
using Knapcode.KitchenSink.Http.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using CloudBlobContainer = Knapcode.KitchenSink.Azure.CloudBlobContainer;
using CloudTable = Knapcode.KitchenSink.Azure.CloudTable;

namespace Knapcode.KitchenSink.Tests.Http.Logging
{
    [TestClass]
    public class AzureHttpMessageStoreTests
    {
        [TestMethod, TestCategory("Integration")]
        public async Task StoreRequestAsync_WithContent_KeepsEquivalentContent()
        {
            // ARRANGE
            var ts = new TestState();
            await ts.Initialize();

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
            var ts = new TestState();
            await ts.Initialize();

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
            var ts = new TestState();
            await ts.Initialize();

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
            var ts = new TestState();
            await ts.Initialize();

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
            var ts = new TestState();
            await ts.Initialize();

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
        public async Task GetRequestAsync_WithValidSessionAndCompression_ReturnsCorrectRequest()
        {
            await VerifyGetRequestAsync(new TestState {UseCompression = true});
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetRequestAsync_WithValidSessionAndNoCompression_ReturnsCorrectRequest()
        {
            await VerifyGetRequestAsync(new TestState {UseCompression = false});
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetRequestAsync_WithInvalidSession_ReturnsNull()
        {
            // ARRANGE
            var ts = new TestState();
            await ts.Initialize();

            // ACT
            HttpRequestMessage request = await ts.Store.GetRequestAsync(ts.Session, CancellationToken.None);

            // ASSERT
            request.Should().BeNull();
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetResponseAsync_WithValidSessionAndCompression_ReturnsCorrectResponse()
        {
            await VerifyGetResponseAsync(new TestState {UseCompression = true});
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetResponseAsync_WithValidSessionAndNoCompression_ReturnsCorrectResponse()
        {
            await VerifyGetResponseAsync(new TestState {UseCompression = false});
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetResponseAsync_WithInvalidSession_ReturnsNull()
        {
            // ARRANGE
            var ts = new TestState();
            await ts.Initialize();

            // ACT
            HttpResponseMessage response = await ts.Store.GetResponseAsync(ts.Session, CancellationToken.None);

            // ASSERT
            response.Should().BeNull();
        }

        private static async Task VerifyGetRequestAsync(TestState ts)
        {
            // ARRANGE
            await ts.Initialize();

            string expectedContent = await ts.RequestWithContent.Content.ReadAsStringAsync();
            KeyValuePair<string, string[]>[] expectedHeaders = CopyHeaders(ts.RequestWithContent.Headers);
            KeyValuePair<string, string[]>[] expectedContentHeaders = CopyHeaders(ts.RequestWithContent.Content.Headers);
            StoredHttpSession session = await ts.Store.StoreRequestAsync(ts.RequestWithContent, CancellationToken.None);

            // ACT
            HttpRequestMessage request = await ts.Store.GetRequestAsync(session, CancellationToken.None);

            // ASSERT
            request.Method.Should().Be(ts.RequestWithContent.Method);
            request.RequestUri.Should().Be(ts.RequestWithContent.RequestUri);
            request.Version.Should().Be(ts.RequestWithContent.Version);
            request.Headers.ShouldBeEquivalentTo(expectedHeaders);
            request.Content.Headers.ShouldBeEquivalentTo(expectedContentHeaders);
            string actualContent = await request.Content.ReadAsStringAsync();
            actualContent.Should().Be(expectedContent);
        }

        private static async Task VerifyGetResponseAsync(TestState ts)
        {
            // ARRANGE
            await ts.Initialize();

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
            string actualContent = await response.Content.ReadAsStringAsync();
            actualContent.Should().Be(expectedContent);
        }

        private static KeyValuePair<string, string[]>[] CopyHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            return headers
                .Select(p => new KeyValuePair<string, string[]>(p.Key, p.Value.ToArray()))
                .ToArray();
        }

        private class TestState
        {
            public TestState()
            {
                Session = new StoredHttpSession {Id = Guid.NewGuid(), Timestamp = DateTimeOffset.UtcNow};

                UseCompression = false;

                CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;

                CloudBlobClient blobClient = account.CreateCloudBlobClient();
                BlobContainer = new CloudBlobContainer(blobClient.GetContainerReference("testcontainer"));

                CloudTableClient tableClient = account.CreateCloudTableClient();
                Table = new CloudTable(tableClient.GetTableReference("testtable"));

                RequestWithContent = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://www.example.com/"),
                    Version = new Version("1.1"),
                    Content = new StringContent("request content", Encoding.UTF8, "text/plain")
                };
                RequestWithContent.Headers.Add("X-Request", "request header");
                RequestWithContent.Content.Headers.Add("X-Request-Content", "request content header");

                RequestWithoutContent = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://www.example.com/"),
                    Version = new Version("1.1")
                };
                RequestWithoutContent.Headers.Add("X-Request", "request header");

                ResponseWithContent = new HttpResponseMessage
                {
                    Version = new Version("1.1"),
                    StatusCode = (HttpStatusCode) 429,
                    ReasonPhrase = "Too Many Requests",
                    Content = new StringContent("response content", Encoding.UTF8, "text/plain")
                };
                ResponseWithContent.Headers.Add("X-Response", "response header");
                ResponseWithContent.Content.Headers.Add("X-Response-Content", "response content header");

                ResponseWithoutContent = new HttpResponseMessage
                {
                    Version = new Version("1.1"),
                    StatusCode = (HttpStatusCode) 429,
                    ReasonPhrase = "Too Many Requests"
                };
                ResponseWithoutContent.Headers.Add("X-Response", "response header");
            }


            public StoredHttpSession Session { get; set; }
            public bool UseCompression { get; set; }
            public ICloudTable Table { get; set; }
            public ICloudBlobContainer BlobContainer { get; set; }
            public AzureHttpMessageStore Store { get; set; }
            public HttpRequestMessage RequestWithContent { get; set; }
            public HttpRequestMessage RequestWithoutContent { get; set; }
            public HttpResponseMessage ResponseWithContent { get; set; }
            public HttpResponseMessage ResponseWithoutContent { get; set; }

            public async Task Initialize()
            {
                Store = new AzureHttpMessageStore(Table, BlobContainer, UseCompression);
                await Table.CreateIfNotExistsAsync(CancellationToken.None);
                await BlobContainer.CreateIfNotExistsAsync(CancellationToken.None);
            }
        }
    }
}