using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Knapcode.KitchenSink.Extensions;
using Knapcode.KitchenSink.Http.Handlers;
using Knapcode.KitchenSink.Http.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Knapcode.KitchenSink.Tests.Http.Handlers
{
    [TestClass]
    public class LoggingHandlerTests
    {
        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithOuterRedirectingHandler_StoredBothSessions()
        {
            // ARRANGE
            var ts = new TestState();
            ts.SetupOuterRedirectingHandler();
            
            // ACT
            var response = await ts.HttpClient.GetAsync("http://example/first");

            // ASSERT
            response.RequestMessage.Should().NotBeNull();

            IEnumerable<StoredHttpSession> storedHttpSessions;
            bool success = response.RequestMessage.TryGetStoredHttpSessions(out storedHttpSessions);
            success.Should().BeTrue();
            storedHttpSessions = storedHttpSessions.ToArray();
            storedHttpSessions.Should().HaveCount(2);
            storedHttpSessions.ElementAt(0).Should().BeSameAs(ts.StoredHttpSessions[0]);
            storedHttpSessions.ElementAt(1).Should().BeSameAs(ts.StoredHttpSessions[1]);
        }
        
        private class TestState
        {
            public TestState()
            {
                StoredHttpSessions = new[] {new StoredHttpSession(), new StoredHttpSession()};
                HttpMessageStoreMock = new Mock<IHttpMessageStore>();
                HttpMessageStoreMock
                    .SetupSequence(h => h.StoreRequestAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(StoredHttpSessions[0]))
                    .Returns(Task.FromResult(StoredHttpSessions[1]));
                InnerHandlerMock = new Mock<HttpMessageHandler>();
                HttpClient = null;
            }

            public StoredHttpSession[] StoredHttpSessions { get; set; }

            public Mock<IHttpMessageStore> HttpMessageStoreMock { get; set; }

            public Mock<HttpMessageHandler> InnerHandlerMock { get; set; }

            public HttpClient HttpClient { get; set; }

            public void SetupOuterRedirectingHandler()
            {
                int callCount = 0;
                InnerHandlerMock
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                    .Returns<HttpRequestMessage, CancellationToken>((request, cancellationToken) =>
                    {
                        callCount++;
                        HttpResponseMessage response;
                        if (callCount == 1)
                        {
                            response = new HttpResponseMessage(HttpStatusCode.Found)
                            {
                                Content = new StringContent(string.Empty),
                                RequestMessage = request
                            };
                            response.Headers.Location = new Uri("http://example/second");
                        }
                        else
                        {
                            response = new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(string.Empty),
                                RequestMessage = request
                            };
                        }

                        return Task.FromResult(response);
                    });

                HttpClient = new HttpClient(new RedirectingHandler
                {
                    InnerHandler = new LoggingHandler(HttpMessageStoreMock.Object)
                    {
                        InnerHandler = InnerHandlerMock.Object
                    }
                });
            }
        }
    }
}
