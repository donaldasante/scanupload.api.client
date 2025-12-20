using Microsoft.AspNetCore.Http;
using ScanUpload.Api.Client.Interface;

namespace ScanUpload.Api.Client.Middleware
{
    public sealed class ScanUploadProxyMiddleware(
        RequestDelegate next,
        IScanUploadProxyService proxyService
    )
    {
        private readonly RequestDelegate _next = next;
        private readonly IScanUploadProxyService _proxyService = proxyService;

        public async Task InvokeAsync(HttpContext context)
        {
            if (await _proxyService.ShouldProxyToTokenApiAsync(context))
            {
                await _proxyService.ProxyRequestToTokenApiAsync(context);
            }
            else if (await _proxyService.ShouldProxyToApiAsync(context))
            {
                await _proxyService.ProxyRequestToApiAsync(context);
            }
            else
            {
                await _next(context);
            }
        }
    }
}
