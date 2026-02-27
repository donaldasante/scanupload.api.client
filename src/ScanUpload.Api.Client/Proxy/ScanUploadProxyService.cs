using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScanUpload.Api.Client.Interface;
using System.Text.Json;

namespace ScanUpload.Api.Client.Proxy
{
    public sealed class ScanUploadProxyService : IScanUploadProxyService
    {
        private static readonly HashSet<string> _restrictedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Host",
            "Connection",
            "Upgrade",
            "Keep-Alive",
            "Proxy-Connection",
            "Transfer-Encoding",
            "Content-Length",
        };

        private readonly HttpClient _httpClient;
        private readonly ScanUploadProxyOptions _options;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<ScanUploadProxyService> _logger;

        public ScanUploadProxyService(
            HttpClient httpClient,
            IOptions<ScanUploadProxyOptions> options,
            ITokenProvider tokenProvider,
            ILogger<ScanUploadProxyService> logger
        )
        {
            _httpClient = httpClient;
            _options = options.Value;
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        public bool ShouldProxyToApi(HttpContext context)
        {
            return
                context.Request.Path.StartsWithSegments(
                    new PathString(_options.ScanUploadRoutePrefix),
                    StringComparison.OrdinalIgnoreCase
                );
        }

        public bool ShouldProxyToTokenApi(HttpContext context)
        {
            if (string.IsNullOrEmpty(_options.ScanUploadTokenRoute))
                return false;

            return string.Equals(
                    context.Request.Path.Value,
                    _options.ScanUploadTokenRoute,
                    StringComparison.OrdinalIgnoreCase
                );
        }

        public async Task ProxyRequestToTokenApiAsync(HttpContext context)
        {
            // For token requests, we can directly get the token and return it
            var tokenResponse = await _tokenProvider.GetAccessTokenAsync();
            context.Response.ContentType = "application/json";

            var payload = new
            {
                access_token = tokenResponse.AccessToken,
                expires_in = tokenResponse.ExpiresIn,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }

        public async Task ProxyRequestToApiAsync(HttpContext context)
        {
            try
            {
                // Build target URL
                var targetUrl = BuildTargetUrl(context);

                // Create proxy request
                var request = CreateProxyRequest(context, targetUrl);

                //Add token to request.
                var bearerToken = await _tokenProvider.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(bearerToken.AccessToken))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer",
                            bearerToken.AccessToken
                        );
                }

                // Send request
                using var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    context.RequestAborted
                );

                // Copy response back to client
                await CopyProxyResponseAsync(context, response);
            }
            catch (TaskCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // Client disconnected
                context.Response.StatusCode = 499;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proxy request failed for {Method} {Path}", context.Request.Method, context.Request.Path);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("An error occurred while processing the request.");
            }
        }

        private string BuildTargetUrl(HttpContext context)
        {
            var path = context.Request.Path.ToString();
            var query = context.Request.QueryString.ToString();

            // Remove route prefix if configured
            if (
                _options.ScanUploadStripRoutePrefix
                && path.StartsWith(_options.ScanUploadRoutePrefix)
            )
            {
                path = path.Substring(_options.ScanUploadRoutePrefix.Length);
                if (string.IsNullOrEmpty(path))
                    path = "/";
            }

            // Ensure path starts with /
            if (!path.StartsWith("/"))
                path = "/" + path;

            return $"{_options.ScanUploadTargetBaseUrl.TrimEnd('/')}{path}{query}";
        }

        private HttpRequestMessage CreateProxyRequest(HttpContext context, string targetUrl)
        {
            var request = context.Request;
            var proxyRequest = new HttpRequestMessage
            {
                Method = new HttpMethod(request.Method),
                RequestUri = new Uri(targetUrl),
            };

            // Copy allowed headers from incoming request
            foreach (var header in request.Headers)
            {
                if (
                    _options.ScanUploadHeadersToForward.Contains(header.Key)
                    && !IsRestrictedHeader(header.Key)
                )
                {
                    if (
                        !proxyRequest.Headers.TryAddWithoutValidation(header.Key, [.. header.Value])
                    )
                    {
                        proxyRequest.Content?.Headers.TryAddWithoutValidation(
                            header.Key,
                            [.. header.Value]
                        );
                    }
                }
            }

            // Add additional headers
            foreach (var header in _options.ScanUploadAdditionalHeaders)
            {
                proxyRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Handle request body
            if (
                request.ContentLength > 0
                && !string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(request.Method, "HEAD", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(request.Method, "DELETE", StringComparison.OrdinalIgnoreCase)
            )
            {
                proxyRequest.Content = new StreamContent(request.Body);

                // Copy content headers
                foreach (var header in request.Headers)
                {
                    if (
                        header.Key.StartsWith("Content-")
                        && _options.ScanUploadHeadersToForward.Contains(header.Key)
                        && !IsRestrictedHeader(header.Key)
                    )
                    {
                        proxyRequest.Content.Headers.TryAddWithoutValidation(
                            header.Key,
                            [.. header.Value]
                        );
                    }
                }
            }

            return proxyRequest;
        }

        private async Task CopyProxyResponseAsync(HttpContext context, HttpResponseMessage response)
        {
            // Copy status code
            context.Response.StatusCode = (int)response.StatusCode;

            // Copy headers
            foreach (var header in response.Headers)
            {
                if (!IsRestrictedHeader(header.Key))
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }
            }

            foreach (var header in response.Content.Headers)
            {
                if (!IsRestrictedHeader(header.Key))
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }
            }

            // Remove headers that should be handled by ASP.NET Core
            context.Response.Headers.Remove("Transfer-Encoding");

            // Copy response body
            using var responseStream = await response.Content.ReadAsStreamAsync();
            await responseStream.CopyToAsync(context.Response.Body);
        }

        private static bool IsRestrictedHeader(string headerName) =>
            _restrictedHeaders.Contains(headerName);
    }
}
