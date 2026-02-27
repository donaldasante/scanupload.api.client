using Microsoft.AspNetCore.Http;

namespace ScanUpload.Api.Client.Interface
{
    public interface IScanUploadProxyService
    {
        bool ShouldProxyToApi(HttpContext context);
        bool ShouldProxyToTokenApi(HttpContext context);

        Task ProxyRequestToApiAsync(HttpContext context);
        Task ProxyRequestToTokenApiAsync(HttpContext context);
    }
}
