using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScanUpload.Api.Client.Interface;
using ScanUpload.Api.Client.Middleware;
using ScanUpload.Api.Client.Proxy;

namespace ScanUpload.Api.Client.Extensions
{
    public static class ScanUploadProxyExtensions
    {
        public static IServiceCollection AddScanUploadProxy(
            this IServiceCollection services,
            Action<ScanUploadProxyOptions> configureOptions
        )
        {
            // Configure options
            services.Configure(configureOptions);

            // Register the proxy service
            services
                .AddHttpClient<IScanUploadProxyService, ScanUploadProxyService>(
                    (serviceProvider, client) =>
                    {
                        var options = serviceProvider
                            .GetRequiredService<IOptions<ScanUploadProxyOptions>>()
                            .Value;
                        client.Timeout = options.ScanUploadRequestTimeout;
                    }
                )
                .SetHandlerLifetime(TimeSpan.FromMinutes(10));
            services.AddKeycloakClient();
            return services;
        }

        public static IApplicationBuilder UseScanUploadProxy(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ScanUploadProxyMiddleware>();
        }

        public static IApplicationBuilder UseScanUploadProxy(
            this IApplicationBuilder app,
            string routePrefix
        )
        {
            return app.UseWhen(
                context => context.Request.Path.StartsWithSegments(routePrefix),
                appBuilder => appBuilder.UseMiddleware<ScanUploadProxyMiddleware>()
            );
        }
    }
}
