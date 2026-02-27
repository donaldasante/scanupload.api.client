using Microsoft.Extensions.Options;
using ScanUpload.Api.Client.Interface;
using ScanUpload.Api.Client.Proxy;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ScanUpload.Api.Client.KeycloakIntegration
{
    public sealed class KeycloakClient : IKeycloakClient, IDisposable
    {
        private readonly HttpClient? _httpClient;
        private readonly ScanUploadProxyOptions _options;
        private readonly bool _ownsHttpClient;
        private bool _disposed = false;

        public KeycloakClient(
            IOptions<ScanUploadProxyOptions> options,
            HttpClient? httpClient = null
        )
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient;
            ValidateConfiguration(_options);

            if (httpClient is null)
            {
                _httpClient = new HttpClient { Timeout = _options.KeycloakTimeout };
                _ownsHttpClient = true;
            }
            else
            {
                _httpClient = httpClient;
                _ownsHttpClient = false;
            }
        }

        public async Task<TokenResponse> GetClientCredentialsTokenAsync(
            CancellationToken cancellationToken = default
        )
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", _options.KeycloakClientId),
                new("client_secret", _options.KeycloakClientSecret),
            };

            if (!string.IsNullOrEmpty(_options.KeycloakScope))
            {
                formData.Add(new KeyValuePair<string, string>("scope", _options.KeycloakScope!));
            }

            using var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue(
                "application/x-www-form-urlencoded"
            );
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                _options.KeycloakTokenEndpoint
            )
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

        private static void ValidateConfiguration(ScanUploadProxyOptions options)
        {
            if (string.IsNullOrEmpty(options.KeycloakServerUrl))
                throw new InvalidOperationException("Keycloak ServerUrl is required");

            if (string.IsNullOrEmpty(options.KeycloakRealm))
                throw new InvalidOperationException("Keycloak Realm is required");

            if (string.IsNullOrEmpty(options.KeycloakClientId))
                throw new InvalidOperationException("Keycloak ClientId is required");

            if (string.IsNullOrEmpty(options.KeycloakClientSecret))
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
