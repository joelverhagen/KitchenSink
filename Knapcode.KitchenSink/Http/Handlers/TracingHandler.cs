using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Knapcode.KitchenSink.Http.Handlers
{
    /// <summary>
    /// A delegating handler that traces request and response information to a text writer.
    /// </summary>
    public class TracingHandler : DelegatingHandler
    {
        private readonly TextWriter _textWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TracingHandler"/> class.
        /// </summary>
        public TracingHandler()
        {
            _textWriter = Console.Out;
            InitializeDefaultSettings();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TracingHandler"/> class.
        /// </summary>
        /// <param name="textWriter">The text writer to write trace messages to.</param>
        public TracingHandler(TextWriter textWriter)
        {
            _textWriter = textWriter;
            InitializeDefaultSettings();
        }

        private void InitializeDefaultSettings()
        {
            WriteFirstRequestLine = true;
            WriteFirstRequestHeaders = false;
            WriteFirstRequestBody = false;

            WriteLastRequestLine = false;
            WriteLastRequestHeaders = false;
            WriteLastRequestBody = false;

            WriteResponseLine = true;
            WriteResponseHeaders = false;
            WriteResponseBody = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the first request line should be written.
        /// </summary>
        public bool WriteFirstRequestLine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the first request headers should be written.
        /// </summary>
        public bool WriteFirstRequestHeaders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the first request body should be written.
        /// </summary>
        public bool WriteFirstRequestBody { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the last request line should be written.
        /// </summary>
        public bool WriteLastRequestLine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the last request headers should be written.
        /// </summary>
        public bool WriteLastRequestHeaders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the last request body should be written.
        /// </summary>
        public bool WriteLastRequestBody { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the response line should be written.
        /// </summary>
        public bool WriteResponseLine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the response headers should be written.
        /// </summary>
        public bool WriteResponseHeaders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the response body should be written.
        /// </summary>
        public bool WriteResponseBody { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (WriteFirstRequestLine)
            {
                WriteRequestLine(request);
            }

            if (WriteFirstRequestHeaders)
            {
                _textWriter.WriteLine(request.Headers);
            }

            if (WriteFirstRequestBody && request.Content != null)
            {
                _textWriter.WriteLine(request.Content.ReadAsStringAsync().Result);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (response.RequestMessage != request)
            {
                if (WriteLastRequestLine)
                {
                    WriteRequestLine(response.RequestMessage);
                }

                if (WriteLastRequestHeaders)
                {
                    _textWriter.WriteLine(response.RequestMessage.Headers);
                }

                if (WriteLastRequestBody && response.RequestMessage.Content != null)
                {
                    _textWriter.WriteLine(response.RequestMessage.Content.ReadAsStringAsync().Result);
                }
            }

            if (WriteResponseLine)
            {
                _textWriter.WriteLine("HTTP/{0} {1} {2}", response.Version.ToString(2), (int)response.StatusCode, response.ReasonPhrase);
            }

            if (WriteResponseHeaders)
            {
                _textWriter.WriteLine(response);
            }

            if (WriteResponseBody && response.Content != null)
            {
                _textWriter.WriteLine(response.Content.ReadAsStringAsync().Result);
            }

            return response;
        }

        private void WriteRequestLine(HttpRequestMessage request)
        {
            _textWriter.WriteLine("{0} {1} HTTP/{2}", request.Method, request.RequestUri, request.Version.ToString(2));
        }
    }
}