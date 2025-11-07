namespace ScanUpload.Api.Client.Proxy
{
    public sealed class ScanUploadProxyOptions
    {
        public string ScanUploadTargetBaseUrl { get; set; } = string.Empty;
        public string ScanUploadRoutePrefix { get; set; } = string.Empty;
        public bool ScanUploadStripRoutePrefix { get; set; }
        public TimeSpan ScanUploadRequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

        // Headers to forward from client request to target
        public string[] ScanUploadHeadersToForward { get; set; } = [];

        // Headers to add to all requests
        public Dictionary<string, string> ScanUploadAdditionalHeaders { get; set; } = [];
        public string KeycloakServerUrl { get; set; } = string.Empty;
        public string KeycloakRealm { get; set; } = string.Empty;
        public string KeycloakClientId { get; set; } = string.Empty;
        public string KeycloakClientSecret { get; set; } = string.Empty;
        public string? KeycloakScope { get; set; }
        public TimeSpan KeycloakTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public int KeycloakEarlyRefreshSeconds { get; set; } = 30;

        public string KeycloakTokenEndpoint =>
            $"{KeycloakServerUrl.TrimEnd('/')}/realms/{KeycloakRealm}/protocol/openid-connect/token";
    }
}
