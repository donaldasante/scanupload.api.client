using Microsoft.AspNetCore.Http;

namespace ScanUpload.Api.Client.Interface
{
    public interface IScanUploadProxyService
    {
        Task<bool> ShouldProxyToApiAsync(HttpContext context);
        Task<bool> ShouldProxyToTokenApiAsync(HttpContext context);

        Task ProxyRequestToApiAsync(HttpContext context);
        Task ProxyRequestToTokenApiAsync(HttpContext context);
    }
}
