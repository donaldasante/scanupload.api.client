using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ScanUpload.Api.Client.KeycloakIntegration
{
    public sealed class KeycloakClient : IDisposable
    {
        private readonly HttpClient? _httpClient;
        private readonly KeycloakOptions _options;
        private readonly bool _ownsHttpClient;
        private bool _disposed = false;

        public KeycloakClient(IOptions<KeycloakOptions> options, HttpClient? httpClient = null)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient;
            ValidateConfiguration(_options);

            if (httpClient is null)
            {
                _httpClient = new HttpClient { Timeout = _options.Timeout };
                _ownsHttpClient = true;
            }
            else
            {
                _httpClient = httpClient;
                _ownsHttpClient = false;
            }

            if (!_options.ServerUrl.EndsWith("/", StringComparison.Ordinal))
                _options.ServerUrl += "/";
        }

        public KeycloakClient(HttpClient httpClient, KeycloakOptions options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            ValidateConfiguration(_options);
        }

        public async Task<TokenResponse> GetClientCredentialsTokenAsync(
            CancellationToken cancellationToken = default
        )
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", _options.ClientId),
                new("client_secret", _options.ClientSecret),
            };

            if (!string.IsNullOrEmpty(_options.Scope))
            {
                formData.Add(new KeyValuePair<string, string>("scope", _options.Scope!));
            }

            using var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue(
                "application/x-www-form-urlencoded"
            );
            using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint)
            {
                Content = content,
            };

            try
            {
                using var response = await _httpClient!
                    .SendAsync(request, cancellationToken)
                    .ConfigureAwait(false);
                var responseContent = await response
                    .Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new KeycloakException(
                        "keycloak_request_failed",
                        $"Keycloak request failed with status {response.StatusCode}: {responseContent}",
                        (int)response.StatusCode
                    );
                }

                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    throw new KeycloakException(
                        "invalid_response",
                        "Failed to deserialize token response"
                    );
                }

                if (!tokenResponse.IsSuccess)
                {
                    throw new KeycloakException(
                        tokenResponse.Error ?? "unknown_error",
                        tokenResponse.ErrorDescription ?? "Token request failed"
                    );
                }

                tokenResponse.ReceivedAtUtc = DateTime.UtcNow;
                return tokenResponse;
            }
            catch (HttpRequestException ex)
            {
                throw new KeycloakException(
                    "http_request_failed",
                    "HTTP request to Keycloak failed",
                    ex
                );
            }
            catch (TaskCanceledException ex)
            {
                throw new KeycloakException("request_timeout", "Request to Keycloak timed out", ex);
            }
            catch (JsonException ex)
            {
                throw new KeycloakException(
                    "invalid_json",
                    "Failed to parse Keycloak response",
                    ex
                );
            }
        }

        private void ValidateConfiguration(KeycloakOptions options)
        {
            if (string.IsNullOrEmpty(options.ServerUrl))
                throw new InvalidOperationException("Keycloak ServerUrl is required");

            if (string.IsNullOrEmpty(options.Realm))
                throw new InvalidOperationException("Keycloak Realm is required");

            if (string.IsNullOrEmpty(options.ClientId))
                throw new InvalidOperationException("Keycloak ClientId is required");

            if (string.IsNullOrEmpty(options.ClientSecret))
                throw new InvalidOperationException("Keycloak ClientSecret is required");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_ownsHttpClient)
                    _httpClient!.Dispose();
                _disposed = true;
            }
        }
    }
}
