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
        public void TryGetRedirectHistory_WithValue_ReturnsTrue()
        {
            TryGetHttpRequestMessageProperty_WithValue_ReturnsTrue(HttpRequestMessageExtensions.TryGetRedirectHistory, RedirectingHandler.RedirectHistoryKey, Enumerable.Empty<HttpResponseMessage>());
        }

        [TestMethod, TestCategory("Unit")]
        public void TryGetRedirectHistory_WithWrongType_ReturnsFalse()
        {
            TryGetHttpRequestMessageProperty_WithWrongType_ReturnsFalse<IEnumerable<HttpResponseMessage>>(HttpRequestMessageExtensions.TryGetRedirectHistory, RedirectingHandler.RedirectHistoryKey);
        }

        [TestMethod, TestCategory("Unit")]
        public void TryGetRedirectHistory_WithNoValue_ReturnsFalse()
        {
            TryGetHttpRequestMessageProperty_WithNoValue_ReturnsFalse<IEnumerable<HttpResponseMessage>>(HttpRequestMessageExtensions.TryGetRedirectHistory);
        }

        [TestMethod, TestCategory("Unit")]
        public void TryGetStoredHttpSession_WithValue_ReturnsTrue()
        {
            TryGetHttpRequestMessageProperty_WithValue_ReturnsTrue(HttpRequestMessageExtensions.TryGetStoredHttpSession, LoggingHandler.StoredHttpSessionKey, new StoredHttpSession());
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

        private static void TryGetHttpRequestMessageProperty_WithValue_ReturnsTrue<T>(TryGetHttpRequestMessageProperty<T> get, string key, T expected)
        {
            // ARRANGE
            var request = new HttpRequestMessage();
            request.Properties.Add(key, expected);
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
