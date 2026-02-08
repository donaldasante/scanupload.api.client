using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScanUpload.Api.Client.ApiClient;
using ScanUpload.Api.Client.Interface;
using ScanUpload.Api.Client.Middleware;
using ScanUpload.Api.Client.Proxy;

namespace ScanUpload.Api.Client.Extensions
{
    public static class ScanUploadProxyExtensions
    {
        public static IServiceCollection AddScanUploadProxy(
            this IServiceCollection services,
            Action<ScanUploadProxyOptions> configureOptions,
            Action<IHttpClientBuilder>? configure = null
        )
        {
            // Configure options
            services.Configure(configureOptions);

            // Register the proxy service
            var builder = services
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
            services.AddKeycloakClient(configure);

            configure?.Invoke(builder);

            return services;
        }

        public static IServiceCollection AddScanUploadApiClient(
            this IServiceCollection services, 
            IConfiguration configuration,
            Action<IHttpClientBuilder>? configure = null)
        {
            services.AddTransient<AuthenticatedHttpClientHandler>();
            var builder = services.AddHttpClient<IScanUploadApiClient, ScanUploadApiClient>(client =>
              {
                  var apiUrl = configuration["ScanUploadProxy:ScanUploadApiClient:ScanUploadBaseUrl"]
                    ?? throw new FileNotFoundException("ScanUpload download URL not found");
                  client.BaseAddress = new Uri(apiUrl);
                  client.DefaultRequestHeaders.Add("Accept", "application/json");
                  client.Timeout = TimeSpan.FromSeconds(120);
              })
              .AddHttpMessageHandler<AuthenticatedHttpClientHandler>();

            configure?.Invoke(builder);

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
