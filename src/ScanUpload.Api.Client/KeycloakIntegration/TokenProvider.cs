using Microsoft.Extensions.Options;
using ScanUpload.Api.Client.Interface;
using ScanUpload.Api.Client.Proxy;

namespace ScanUpload.Api.Client.KeycloakIntegration
{
    public sealed class TokenProvider(
        KeycloakClient keycloakClient,
        IOptions<ScanUploadProxyOptions> options
    ) : ITokenProvider, IDisposable
    {
        private volatile TokenResponse? _cached; // fast read by multiple threads
        private readonly KeycloakClient _keycloakClient = keycloakClient;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private readonly ScanUploadProxyOptions _options = options.Value;

        public async Task<TokenResponse> GetAccessTokenAsync(
            CancellationToken cancellationToken = default
        )
        {
            // Fast path: if cached token exists and is still valid (consider early refresh)
            var current = _cached;
            if (current != null && !current.IsExpired(_options.KeycloakEarlyRefreshSeconds))
                return current;

            // Slow path: refresh guarded by a semaphore to prevent stampedes
            await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Double-check after acquiring the lock
                current = _cached;
                if (current != null && !current.IsExpired(_options.KeycloakEarlyRefreshSeconds))
                    return current;

                _cached = await _keycloakClient
                    .GetClientCredentialsTokenAsync(cancellationToken)
                    .ConfigureAwait(false);
                return _cached;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        public void Dispose()
        {
            _refreshLock?.Dispose();
        }
    }
}
