using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Knapcode.KitchenSink.Extensions
{
    public static class HttpHeadersExtensions
    {
        public static void AddRange(this HttpHeaders destination, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            foreach (var pair in headers)
            {
                destination.Add(pair.Key, pair.Value);
            }
        }
    }
}
