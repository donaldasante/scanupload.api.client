using System;
using System.Collections.Generic;
using System.Text;

namespace ScanUpload.Api.Client.KeycloakIntegration
{
    public sealed class KeycloakOptions
    {
        public string ServerUrl { get; set; } = string.Empty; // e.g. "https://auth.example.com"
        public string Realm { get; set; } = string.Empty; // e.g. "myrealm"
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string? Scope { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public int EarlyRefreshSeconds { get; set; } = 30;

        public string TokenEndpoint =>
            $"{ServerUrl.TrimEnd('/')}/realms/{Realm}/protocol/openid-connect/token";
    }
}
