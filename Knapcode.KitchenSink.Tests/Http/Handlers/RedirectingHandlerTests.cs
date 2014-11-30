﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Knapcode.KitchenSink.Http.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Knapcode.KitchenSink.Tests.Http.Handlers
{
    [TestClass]
    public class RedirectingHandlerTests
    {
        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithTooManyRedirects_StopsRedirecting()
        {
            // ARRANGE
            const HttpStatusCode statusCode = HttpStatusCode.TemporaryRedirect;
            var client = GetHttpClient(
                configure: handler => handler.MaxAutomaticRedirections = 5,
                redirectCount: 6,
                statusCode: statusCode);
            var request = GetRequest();

            // ACT
            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);

            // ASSERT
            httpResponseMessage.StatusCode.Should().Be(statusCode);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithNoLocationHeader_DoesNotRedirect()
        {
            // ARRANGE
            const HttpStatusCode statusCode = HttpStatusCode.TemporaryRedirect;
            var client = GetHttpClient(
                redirectUri: new Uri(string.Empty, UriKind.Relative),
                statusCode: statusCode);
            var request = GetRequest();

            // ACT
            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);

            // ASSERT
            httpResponseMessage.StatusCode.Should().Be(statusCode);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithDisabledRedirect_DoesNotRedirect()
        {
            // ARRANGE
            const HttpStatusCode statusCode = HttpStatusCode.TemporaryRedirect;
            var client = GetHttpClient(
                configure: handler => handler.AllowAutoRedirect = false,
                statusCode: statusCode);
            var request = GetRequest();

            // ACT
            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);

            // ASSERT
            httpResponseMessage.StatusCode.Should().Be(statusCode);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithDisabledHistory_DoesKeepHistory()
        {
            // ARRANGE
            var client = GetHttpClient(configure: handler => handler.KeepRedirectHistory = false);
            var request = GetRequest();

            // ACT
            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);

            // ASSERT
            httpResponseMessage.RequestMessage.Properties.ContainsKey(RedirectingHandler.RedirectHistoryKey).Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithContentHeaders_CopiesContentHeaders()
        {
            // ARRANGE
            var client = GetHttpClient(HttpStatusCode.TemporaryRedirect);
            var request = GetRequest();
            request.Method = HttpMethod.Post;
            request.Content = new StringContent("foo");

            const string headerKey = "X-Foo";
            const string headerValue = "bar";
            request.Content.Headers.Add(headerKey, headerValue);

            // ACT
            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);

            // ASSERT
            string[] values = httpResponseMessage.RequestMessage.Content.Headers.GetValues(headerKey).ToArray();
            values.Should().HaveCount(1);
            values.Should().BeEquivalentTo(headerValue);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithRedirects_KeepsHistory()
        {
            // ARRANGE
            const int redirectCount = 5;
            const HttpStatusCode statusCode = HttpStatusCode.TemporaryRedirect;
            var client = GetHttpClient(redirectCount: redirectCount, statusCode: statusCode);
            var request = GetRequest();

            // ACT
            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);

            // ASSERT
            httpResponseMessage.RequestMessage.Properties.ContainsKey(RedirectingHandler.RedirectHistoryKey).Should().BeTrue();
            object value = httpResponseMessage.RequestMessage.Properties[RedirectingHandler.RedirectHistoryKey];
            value.Should().BeAssignableTo<IEnumerable<HttpResponseMessage>>();
            HttpResponseMessage[] responses = ((IEnumerable<HttpResponseMessage>) value).ToArray();
            responses.Should().HaveCount(redirectCount + 1);
            responses.Take(redirectCount).Should().OnlyContain(r => r.StatusCode == statusCode);
            responses.Skip(redirectCount).Take(1).Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithHeaders_CopiesHeaders()
        {
            // ARRANGE
            var client = GetHttpClient();
            var request = GetRequest();

            const string headerKey = "X-Foo";
            const string headerValue = "bar";
            request.Headers.Add(headerKey, headerValue);

            // ACT
            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);

            // ASSERT
            string[] values = httpResponseMessage.RequestMessage.Headers.GetValues(headerKey).ToArray();
            values.Should().HaveCount(1);
            values.Should().BeEquivalentTo(headerValue);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithNoSchemeInRedirect_UsesRequestUriScheme()
        {
            // ARRANGE
            var client = GetHttpClient(redirectUri: new Uri("//www.example.com/2", UriKind.Relative));
            var request = GetRequest();
            request.RequestUri = new Uri("https://www.example.com/1");

            // ACT
            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);

            // ASSERT
            httpResponseMessage.RequestMessage.RequestUri.Should().Be(new Uri("https://www.example.com/2"));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithRelativeRedirect_ResolvedAgainstRequestUri()
        {
            // ARRANGE
            var client = GetHttpClient(redirectUri: new Uri("../c/e/../d.txt", UriKind.Relative));
            var request = GetRequest();
            request.RequestUri = new Uri("https://www.example.com/1/2/3/4.txt");

            // ACT
            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);

            // ASSERT
            httpResponseMessage.RequestMessage.RequestUri.Should().Be(new Uri("https://www.example.com/1/2/c/d.txt"));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithPostAnd301_MakesGetRequest()
        {
            await SendAsync_WithRedirect_MakesSubsequentRequest(
                HttpMethod.Post,
                "foo",
                HttpStatusCode.Moved,
                HttpMethod.Get,
                null);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithPutAnd301_DuplicatesRequestWithoutContent()
        {
            await SendAsync_WithRedirect_MakesSubsequentRequest(
                HttpMethod.Put,
                "foo",
                HttpStatusCode.Moved,
                HttpMethod.Put,
                null);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithDeleteAnd301_DuplicatesRequestWithoutContent()
        {
            await SendAsync_WithRedirect_MakesSubsequentRequest(
                HttpMethod.Delete,
                "foo",
                HttpStatusCode.Moved,
                HttpMethod.Delete,
                null);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithGetAnd301_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Get, null, HttpStatusCode.Moved);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithHeadAnd301_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Head, null, HttpStatusCode.Moved);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithPostAnd302_MakesGetRequest()
        {
            await SendAsync_WithRedirect_MakesSubsequentRequest(
                HttpMethod.Post,
                "foo",
                HttpStatusCode.Found,
                HttpMethod.Get,
                null);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithPutAnd302_MakesGetRequest()
        {
            await SendAsync_WithRedirect_MakesSubsequentRequest(
                HttpMethod.Put,
                "foo",
                HttpStatusCode.Found,
                HttpMethod.Get,
                null);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithDeleteAnd302_MakesGetRequest()
        {
            await SendAsync_WithRedirect_MakesSubsequentRequest(
                HttpMethod.Delete,
                "foo",
                HttpStatusCode.Found,
                HttpMethod.Get,
                null);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithGetAnd302_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Get, null, HttpStatusCode.Found);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithHeadAnd302_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Head, null, HttpStatusCode.Found);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithPostAnd303_MakesGetRequest()
        {
            await SendAsync_WithRedirect_MakesSubsequentRequest(
                HttpMethod.Post,
                "foo",
                HttpStatusCode.SeeOther,
                HttpMethod.Get,
                null);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithPutAnd303_MakesGetRequest()
        {
            await SendAsync_WithRedirect_MakesSubsequentRequest(
                HttpMethod.Put,
                "foo",
                HttpStatusCode.SeeOther,
                HttpMethod.Get,
                null);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithDeleteAnd303_MakesGetRequest()
        {
            await SendAsync_WithRedirect_MakesSubsequentRequest(
                HttpMethod.Delete,
                "foo",
                HttpStatusCode.SeeOther,
                HttpMethod.Get,
                null);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithGetAnd303_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Get, null, HttpStatusCode.SeeOther);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithHeadAnd303_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Head, null, HttpStatusCode.SeeOther);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithPostAnd307_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Post, "foo", HttpStatusCode.TemporaryRedirect);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithPutAnd307_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Put, "foo", HttpStatusCode.TemporaryRedirect);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithDeleteAnd307_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Delete, "foo", HttpStatusCode.TemporaryRedirect);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithGetAnd307_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Get, null, HttpStatusCode.TemporaryRedirect);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithHeadAnd307_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Head, null, HttpStatusCode.TemporaryRedirect);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithPostAnd308_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Post, "foo", (HttpStatusCode)308);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithPutAnd308_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Put, "foo", (HttpStatusCode)308);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithDeleteAnd308_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Delete, "foo", (HttpStatusCode)308);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithGetAnd308_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Get, null, (HttpStatusCode)308);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendAsync_WithHeadAnd308_DuplicatesRequest()
        {
            await SendAsync_WithRedirect_DuplicatesRequest(HttpMethod.Head, null, (HttpStatusCode)308);
        }

        private static async Task SendAsync_WithRedirect_DuplicatesRequest(HttpMethod httpMethod, string content, HttpStatusCode httpStatusCode)
        {
            await SendAsync_WithRedirect_MakesSubsequentRequest(
                httpMethod,
                content,
                httpStatusCode,
                httpMethod,
                content);
        }

        private static async Task SendAsync_WithRedirect_MakesSubsequentRequest(HttpMethod initialMethod, string initialContent, HttpStatusCode httpStatusCode, HttpMethod expectedMethod, string expectedContent)
        {
            var content = new StubbedHttpContent(string.Empty);
            if (initialContent != null)
            {
                content = new StubbedHttpContent(initialContent);
            }

            await SendAsync_WithRedirect_MakesNewRequest(
                initialMethod,
                content,
                httpStatusCode,
                async request =>
                {
                    request.Method.Should().Be(expectedMethod);
                    if (expectedContent == null)
                    {
                        request.Content.Should().BeNull();
                    }
                    else
                    {
                        content.SerializeToStreamAsyncCalls.Should().Be(1);
                        request.Content.Should().NotBeNull();
                        string actualContent = await request.Content.ReadAsStringAsync();
                        actualContent.Should().Be(expectedContent);
                    }
                });
        }

        private static async Task SendAsync_WithRedirect_MakesNewRequest(HttpMethod initialMethod, HttpContent content, HttpStatusCode statusCode, Action<HttpRequestMessage> validateRequest)
        {
            // ARRANGE
            var client = GetHttpClient(statusCode);
            var request = GetRequest();
            request.Method = initialMethod;
            request.Content = content;

            // ACT
            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);

            // ASSERT
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
            validateRequest(httpResponseMessage.RequestMessage);
        }

        private static HttpResponseMessage GetOkHttpResponseMessage()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private static HttpResponseMessage GetRedirectHttpResponseMessage(HttpStatusCode statusCode, Uri redirectUri)
        {
            var response = new HttpResponseMessage(statusCode);
            response.Headers.Location = redirectUri;
            return response;
        }

        private static Mock<HttpMessageHandler> GetHttpMessageHandlerMock(Queue<HttpResponseMessage> responseQueue)
        {
            var mock = new Mock<HttpMessageHandler>();

            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns<HttpRequestMessage, CancellationToken>((request, token) =>
                {
                    HttpResponseMessage response = responseQueue.Dequeue();
                    response.RequestMessage = request;
                    return Task.FromResult(response);
                });

            return mock;
        }

        private static HttpRequestMessage GetRequest()
        {
            return new HttpRequestMessage(HttpMethod.Get, "http://www.example.com/1");
        }

        private static HttpClient GetHttpClient(HttpStatusCode statusCode = HttpStatusCode.Moved, Uri redirectUri = null, int redirectCount = 1, Action<RedirectingHandler> configure = null)
        {
            if (redirectUri == null)
            {
                redirectUri = new Uri("http://www.example.com/2", UriKind.Absolute);
            }

            var responses = Enumerable
                .Range(0, redirectCount)
                .Select(i => GetRedirectHttpResponseMessage(statusCode, redirectUri))
                .Concat(new[]
            {
                GetOkHttpResponseMessage()
            });

            var responseQueue = new Queue<HttpResponseMessage>(responses);

            Mock<HttpMessageHandler> httpMessageHandlerMock = GetHttpMessageHandlerMock(responseQueue);

            var handler = new RedirectingHandler { InnerHandler = httpMessageHandlerMock.Object };
            if (configure != null)
            {
                configure(handler);
            }

            var client = new HttpClient(handler);
            return client;
        }

        private class StubbedHttpContent : StringContent
        {
            public StubbedHttpContent(string content)
                : base(content)
            {
            }

            public int SerializeToStreamAsyncCalls { get; private set; }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                SerializeToStreamAsyncCalls++;
                return base.SerializeToStreamAsync(stream, context);
            }
        }
    }
}