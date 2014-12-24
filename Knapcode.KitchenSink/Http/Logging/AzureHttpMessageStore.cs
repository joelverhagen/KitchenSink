using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using Knapcode.KitchenSink.Azure;
using Knapcode.KitchenSink.Extensions;
using Knapcode.KitchenSink.Support;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Knapcode.KitchenSink.Http.Logging
{
    public class AzureHttpMessageStore : IHttpMessageStore
    {
        private const int BufferSize = 4096;
        private const string RequestRowKeySuffix = "request";
        private const string ResponseRowKeySuffix = "response";
        private readonly ICloudBlobContainer _blobContainer;
        private readonly ICloudTable _table;
        private readonly bool _useCompression;

        public AzureHttpMessageStore(ICloudTable table, ICloudBlobContainer blobContainer, bool useCompression)
        {
            Guard.ArgumentNotNull(table, "table");
            Guard.ArgumentNotNull(blobContainer, "blobContainer");

            _table = table;
            _blobContainer = blobContainer;
            _useCompression = useCompression;
        }

        public async Task<StoredHttpSession> StoreRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNull(request, "request");

            var session = new StoredHttpSession {Timestamp = DateTimeOffset.UtcNow, Id = Guid.NewGuid()};

            string partitionKey = GetPartitionKey(session);
            string rowKey = GetRequestRowKey(session);

            request.Content = await StoreContentAsync(rowKey, request.Content, cancellationToken);

            var entity = new HttpRequestMessageEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                Method = request.Method.Method,
                RequestUri = request.RequestUri.ToString(),
                Version = request.Version.ToString(),
                Headers = SerializeHeaders(request.Headers),
                ContentHeaders = request.Content != null ? SerializeHeaders(request.Content.Headers) : null,
                HasContent = request.Content != null,
                IsContentCompressed = _useCompression
            };
            await _table.ExecuteAsync(TableOperation.Insert(entity), cancellationToken);

            return session;
        }

        public async Task StoreResponseAsync(StoredHttpSession session, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNull(session, "session");
            Guard.ArgumentNotNull(response, "response");

            string partitionKey = GetPartitionKey(session);
            string rowKey = GetResponseRowKey(session);

            response.Content = await StoreContentAsync(rowKey, response.Content, cancellationToken);
            var entity = new HttpResponseMessageEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                Version = response.Version.ToString(),
                StatusCode = (int) response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = SerializeHeaders(response.Headers),
                ContentHeaders = response.Content != null ? SerializeHeaders(response.Content.Headers) : null,
                HasContent = response.Content != null,
                IsContentCompressed = _useCompression
            };
            await _table.ExecuteAsync(TableOperation.Insert(entity), cancellationToken);
        }

        public async Task<HttpRequestMessage> GetRequestAsync(StoredHttpSession session, CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNull(session, "session");

            string partitionKey = GetPartitionKey(session);
            string rowKey = GetRequestRowKey(session);

            TableResult result = await _table.ExecuteAsync(TableOperation.Retrieve<HttpRequestMessageEntity>(partitionKey, rowKey), cancellationToken);
            if (result.Result == null)
            {
                return null;
            }

            var entity = (HttpRequestMessageEntity) result.Result;
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(entity.Method),
                RequestUri = new Uri(entity.RequestUri),
                Version = new Version(entity.Version)
            };
            request.Headers.AddRange(DeserializeHeaders(entity.Headers));
            if (entity.HasContent)
            {
                request.Content = await GetStoredHttpContentAsync(rowKey, DeserializeHeaders(entity.ContentHeaders), entity.IsContentCompressed, cancellationToken);
            }

            return request;
        }

        public async Task<HttpResponseMessage> GetResponseAsync(StoredHttpSession session, CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNull(session, "session");

            string partitionKey = GetPartitionKey(session);
            string rowKey = GetResponseRowKey(session);

            TableResult result = await _table.ExecuteAsync(TableOperation.Retrieve<HttpResponseMessageEntity>(partitionKey, rowKey), cancellationToken);
            if (result.Result == null)
            {
                return null;
            }

            var entity = (HttpResponseMessageEntity) result.Result;
            var response = new HttpResponseMessage
            {
                Version = new Version(entity.Version),
                StatusCode = (HttpStatusCode) entity.StatusCode,
                ReasonPhrase = entity.ReasonPhrase,
            };
            response.Headers.AddRange(DeserializeHeaders(entity.Headers));
            if (entity.HasContent)
            {
                response.Content = await GetStoredHttpContentAsync(rowKey, DeserializeHeaders(entity.ContentHeaders), entity.IsContentCompressed, cancellationToken);
            }

            return response;
        }

        private async Task<HttpContent> StoreContentAsync(string rowKey, HttpContent originalContent, CancellationToken cancellationToken)
        {
            HttpContent storedContent = null;
            if (originalContent != null)
            {
                ICloudBlockBlob blob = _blobContainer.GetBlockBlobReference(rowKey);
                Stream originalStream = await originalContent.ReadAsStreamAsync();

                Stream destinationStream = await blob.OpenWriteAsync(cancellationToken);
                if (_useCompression)
                {
                    destinationStream = new BufferedStream(new GZipOutputStream(destinationStream), BufferSize);
                }

                using (destinationStream)
                {
                    await originalStream.CopyToAsync(destinationStream, BufferSize, cancellationToken);
                }

                storedContent = await GetStoredHttpContentAsync(rowKey, originalContent.Headers, _useCompression, cancellationToken);
            }

            return storedContent;
        }

        private async Task<HttpContent> GetStoredHttpContentAsync(string rowKey, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers, bool isCompressed, CancellationToken cancellationToken)
        {
            ICloudBlockBlob blob = _blobContainer.GetBlockBlobReference(rowKey);
            Stream stream = await blob.OpenReadAsync(cancellationToken);

            if (isCompressed)
            {
                stream = new GZipInputStream(stream);
            }

            var httpContent = new StreamContent(stream);
            httpContent.Headers.AddRange(headers);
            return httpContent;
        }

        private static string GetRequestRowKey(StoredHttpSession session)
        {
            return GetRowKey(session, RequestRowKeySuffix);
        }

        private static string GetResponseRowKey(StoredHttpSession session)
        {
            return GetRowKey(session, ResponseRowKeySuffix);
        }

        private static string GetRowKey(StoredHttpSession session, string suffix)
        {
            return string.Join("-", new[] {session.Timestamp.GetDescendingOrderString(), Base64Url.Encode(session.Id.ToByteArray()), suffix});
        }

        private static string GetPartitionKey(StoredHttpSession session)
        {
            return DateTimeOffset.MinValue.GetDescendingOrderString();
        }

        private static string SerializeHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            return JsonConvert.SerializeObject(headers);
        }

        private static IEnumerable<KeyValuePair<string, IEnumerable<string>>> DeserializeHeaders(string serialized)
        {
            return JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, IEnumerable<string>>>>(serialized);
        }

        private class HttpRequestMessageEntity : LoggedHttpMessageEntity
        {
            public string Method { get; set; }
            public string RequestUri { get; set; }
            public string Version { get; set; }
            public string Headers { get; set; }
        }

        private class HttpResponseMessageEntity : LoggedHttpMessageEntity
        {
            public string Version { get; set; }
            public int StatusCode { get; set; }
            public string ReasonPhrase { get; set; }
            public string Headers { get; set; }
        }

        private class LoggedHttpMessageEntity : TableEntity
        {
            public string ContentHeaders { get; set; }
            public bool HasContent { get; set; }
            public bool IsContentCompressed { get; set; }
        }
    }
}