using System.Text.Json.Serialization;

namespace ScanUpload.Api.Client.KeycloakIntegration
{
    public sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_expires_in")]
        public int RefreshExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "bearer";

        [JsonPropertyName("not-before-policy")]
        public int NotBeforePolicy { get; set; }

        [JsonPropertyName("session_state")]
        public string? SessionState { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }

        public bool IsSuccess => !string.IsNullOrEmpty(AccessToken) && string.IsNullOrEmpty(Error);
        public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsExpired(int earlyRefreshSeconds = 0, DateTime? nowUtc = null)
        {
            var now = nowUtc ?? DateTime.UtcNow;
            var margin = Math.Max(0, earlyRefreshSeconds);
            // Consider a small safety margin
            return now >= ReceivedAtUtc.AddSeconds(Math.Max(0, ExpiresIn - margin));
        }
    }
}
