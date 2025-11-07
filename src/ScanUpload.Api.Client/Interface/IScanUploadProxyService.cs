using Microsoft.AspNetCore.Http;

namespace ScanUpload.Api.Client.Interface
{
    public interface IScanUploadProxyService
    {
        Task<bool> ShouldProxyAsync(HttpContext context);
        Task ProxyRequestAsync(HttpContext context);
    }
}
