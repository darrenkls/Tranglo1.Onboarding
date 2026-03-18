using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Infrastructure.Commons
{
    public class LoggingHandler<T> : DelegatingHandler
    {
        private readonly ILogger<T> _logger;

        public LoggingHandler(ILogger<T> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestContent = await ReadContentAsync(request.Content);
            var logBuffer = new StringBuilder();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                LogRequest(request, requestContent, logBuffer);
            }

            var stopwatch = Stopwatch.StartNew();
            HttpResponseMessage response = null;

            try
            {
                response = await base.SendAsync(request, cancellationToken);

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    var responseContent = await ReadContentAsync(response.Content);
                    LogResponse(response, responseContent, logBuffer);
                }

                return response;
            }
            finally
            {
                stopwatch.Stop();

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    logBuffer.AppendLine();
                    logBuffer.AppendLine($"Elapsed = {stopwatch.Elapsed}");
                    _logger.LogDebug(logBuffer.ToString());
                }
            }
        }

        #region Private Helper Methods
        private static async Task<string> ReadContentAsync(HttpContent content)
        {
            return content != null ? await content.ReadAsStringAsync() : string.Empty;
        }

        private static void LogRequest(HttpRequestMessage request, string content, StringBuilder buffer)
        {
            buffer.AppendLine();
            buffer.AppendLine($"{request.Method} {request.RequestUri}");

            foreach (var header in request.Headers)
            {
                buffer.AppendLine($"{header.Key}: {MaskAuthorizationHeader(header)}");
            }

            buffer.AppendLine();
            if (!string.IsNullOrEmpty(content))
            {
                buffer.AppendLine(content);
            }
        }

        private static void LogResponse(HttpResponseMessage response, string content, StringBuilder buffer)
        {
            buffer.AppendLine();
            buffer.AppendLine($"HTTP/{response.Version} {response.StatusCode}");

            foreach (var header in response.Headers)
            {
                buffer.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            buffer.AppendLine();
            if (!string.IsNullOrEmpty(content))
            {
                buffer.AppendLine(content);
            }
        }

        private static string MaskAuthorizationHeader(KeyValuePair<string, IEnumerable<string>> header)
        {
            return header.Key.Equals("authorization", StringComparison.OrdinalIgnoreCase)
                ? "******"
                : string.Join(", ", header.Value);
        }
        #endregion Private Helper Methods
    }
}
