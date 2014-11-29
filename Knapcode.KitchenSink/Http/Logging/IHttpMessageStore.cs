using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Knapcode.KitchenSink.Http.Logging
{
    public interface IHttpMessageStore
    {
        /// <summary>
        /// Store a request to the store. The request's properties should be left semantically equivalent. For example,
        /// the request content can be changed to read some a different source.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The session identifier.</returns>
        Task<StoredHttpSession> StoreRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>
        /// Store a response to the store. The response's properties should be  left semantically equivalent. For
        /// example, the response content can be changed to read some a different source.
        /// </summary>
        /// <param name="session">The session identifier, typically returned from <see cref="StoreRequestAsync"/>.</param>
        /// <param name="response">Thre response.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task StoreResponseAsync(StoredHttpSession session, HttpResponseMessage response, CancellationToken cancellationToken);

        /// <summary>
        /// Get a request from the store.
        /// </summary>
        /// <param name="session">The session identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The request.</returns>
        Task<HttpRequestMessage> GetRequestAsync(StoredHttpSession session, CancellationToken cancellationToken);

        /// <summary>
        /// Get a response from the store.
        /// </summary>
        /// <param name="session">The session identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        Task<HttpResponseMessage> GetResponseAsync(StoredHttpSession session, CancellationToken cancellationToken);
    }
}