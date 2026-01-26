using ScanUpload.Api.Client.Interface;
using System.Net.Http.Headers;

namespace ScanUpload.Api.Client.Middleware
{
    public sealed class AuthenticatedHttpClientHandler(ITokenProvider tokenProvider) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage request,
          CancellationToken cancellationToken
        )
        {
            if (request is null) { throw new ArgumentNullException(nameof(request)); }
            var token = await tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
