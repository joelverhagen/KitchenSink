using System;

namespace Knapcode.KitchenSink.Http.Logging
{
    public class StoredHttpSession
    {
        public DateTimeOffset Timestamp { get; set; }
        public Guid Id { get; set; }
    }
}
