using ScanUpload.Api.Client.KeycloakIntegration;

namespace ScanUpload.Api.Client.Interface
{
    public interface IKeycloakClient
    {
        Task<TokenResponse> GetClientCredentialsTokenAsync(CancellationToken cancellationToken = default);
    }
}
