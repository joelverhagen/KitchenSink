using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Knapcode.KitchenSink.Http.Logging;

namespace Knapcode.KitchenSink.Http.Handlers
{
    public class LoggingHandler : DelegatingHandler
    {
        private readonly IHttpMessageStore _store;

        public LoggingHandler(IHttpMessageStore store)
        {
            _store = store;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            StoredHttpSession session = await _store.StoreRequestAsync(request, cancellationToken);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            await _store.StoreResponseAsync(session, response, cancellationToken);
            return response;
        }
    }
}