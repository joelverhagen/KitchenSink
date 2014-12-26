using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Knapcode.KitchenSink.Extensions;
using Knapcode.KitchenSink.Http.Logging;

namespace Knapcode.KitchenSink.Http.Handlers
{
    public class LoggingHandler : DelegatingHandler
    {
        /// <summary>
        /// The property key used to access the list of responses in <see cref="HttpRequestMessage.Properties"/>.
        /// </summary>
        public const string StoredHttpSessionKey = "Knapcode.KitchenSink.Http.Handlers.LoggingHandler.StoredHttpSession";

        private readonly IHttpMessageStore _store;

        public LoggingHandler(IHttpMessageStore store)
        {
            _store = store;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            StoredHttpSession session = await _store.StoreRequestAsync(request, cancellationToken);

            IEnumerable<StoredHttpSession> storedHttpSessions;
            var storedHttpSessionList = new List<StoredHttpSession>();
            if (request.TryGetStoredHttpSessions(out storedHttpSessions))
            {
                storedHttpSessionList = storedHttpSessions.ToList();
            }

            storedHttpSessionList.Add(session);
            request.Properties.Remove(StoredHttpSessionKey);
            request.Properties.Add(StoredHttpSessionKey, storedHttpSessionList);
                        
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            await _store.StoreResponseAsync(session, response, cancellationToken);
            return response;
        }
    }
}