using ScanUpload.Api.Client.KeycloakIntegration;

namespace ScanUpload.Api.Client.Interface
{
    public interface ITokenProvider
    {
        Task<TokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
