using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Knapcode.KitchenSink.Extensions;
using Knapcode.KitchenSink.Http.Handlers;
using Knapcode.KitchenSink.Http.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Knapcode.KitchenSink.Tests.Extensions
{
    [TestClass]
    public class HttpRequestMessageExtensionsTests
    {
        private delegate bool TryGetHttpRequestMessageProperty<T>(HttpRequestMessage request, out T value);

        [TestMethod, TestCategory("Unit")]
        public void TryGetStoredHttpSession_WithValue_ReturnsTrue()
        {
            var session = new StoredHttpSession();
            TryGetHttpRequestMessageProperty_WithValue_ReturnsTrue(HttpRequestMessageExtensions.TryGetStoredHttpSession, LoggingHandler.StoredHttpSessionKey, session, new[] { session });
        }

        [TestMethod, TestCategory("Unit")]
        public void TryGetStoredHttpSession_WithWrongType_ReturnsFalse()
        {
            TryGetHttpRequestMessageProperty_WithWrongType_ReturnsFalse<StoredHttpSession>(HttpRequestMessageExtensions.TryGetStoredHttpSession, LoggingHandler.StoredHttpSessionKey);
        }

        [TestMethod, TestCategory("Unit")]
        public void TryGetStoredHttpSession_WithNoValue_ReturnsFalse()
        {
            TryGetHttpRequestMessageProperty_WithNoValue_ReturnsFalse<StoredHttpSession>(HttpRequestMessageExtensions.TryGetStoredHttpSession);
        }

        [TestMethod, TestCategory("Unit")]
        public void TryGetStoredHttpSessions_WithValue_ReturnsTrue()
        {
            TryGetHttpRequestMessageProperty_WithValue_ReturnsTrue(HttpRequestMessageExtensions.TryGetStoredHttpSessions, LoggingHandler.StoredHttpSessionKey, Enumerable.Empty<StoredHttpSession>());
        }

        [TestMethod, TestCategory("Unit")]
        public void TryGetStoredHttpSessions_WithWrongType_ReturnsFalse()
        {
            TryGetHttpRequestMessageProperty_WithWrongType_ReturnsFalse<IEnumerable<StoredHttpSession>>(HttpRequestMessageExtensions.TryGetStoredHttpSessions, LoggingHandler.StoredHttpSessionKey);
        }

        [TestMethod, TestCategory("Unit")]
        public void TryGetStoredHttpSessions_WithNoValue_ReturnsFalse()
        {
            TryGetHttpRequestMessageProperty_WithNoValue_ReturnsFalse<IEnumerable<StoredHttpSession>>(HttpRequestMessageExtensions.TryGetStoredHttpSessions);
        }

        private static void TryGetHttpRequestMessageProperty_WithValue_ReturnsTrue<T>(TryGetHttpRequestMessageProperty<T> get, string key, T expected, object addedValue = null) where T : class
        {
            // ARRANGE
            if (addedValue == null)
            {
                addedValue = expected;
            }

            var request = new HttpRequestMessage();
            request.Properties.Add(key, addedValue);
            T actual;

            // ACT
            bool success = get(request, out actual);

            // ASSERT
            success.Should().BeTrue();
            actual.Should().BeSameAs(expected);
        }

        private static void TryGetHttpRequestMessageProperty_WithWrongType_ReturnsFalse<T>(TryGetHttpRequestMessageProperty<T> get, string key)
        {
            // ARRANGE
            var request = new HttpRequestMessage();
            request.Properties.Add(key, typeof(T) == typeof(string) ? (object) 23 : "23");
            T actual;

            // ACT
            bool success = get(request, out actual);

            // ASSERT
            success.Should().BeFalse();
            actual.Should().Be(default(T));
        }

        private static void TryGetHttpRequestMessageProperty_WithNoValue_ReturnsFalse<T>(TryGetHttpRequestMessageProperty<T> get)
        {
            // ARRANGE
            var request = new HttpRequestMessage();
            T actual;

            // ACT
            bool success = get(request, out actual);

            // ASSERT
            success.Should().BeFalse();
            actual.Should().Be(default(T));
        }
    }
}
