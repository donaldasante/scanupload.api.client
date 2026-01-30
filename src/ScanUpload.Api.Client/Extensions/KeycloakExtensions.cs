using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ScanUpload.Api.Client.Interface;
using ScanUpload.Api.Client.KeycloakIntegration;
using ScanUpload.Api.Client.Proxy;

namespace ScanUpload.Api.Client.Extensions
{
    internal static class KeycloakExtensions
    {
        internal static IServiceCollection AddKeycloakClient(
            this IServiceCollection services,
            Action<ScanUploadProxyOptions> configureOptions, 
            Action<IHttpClientBuilder>? configure = null
        )
        {
            services.Configure(configureOptions);
            var builder = services
                .AddHttpClient<KeycloakClient>(
                    (serviceProvider, client) =>
                    {
                        var options = serviceProvider
                            .GetRequiredService<IOptions<ScanUploadProxyOptions>>()
                            .Value;
                        client.Timeout = options.KeycloakTimeout;
                    }
                )
                .SetHandlerLifetime(TimeSpan.FromMinutes(10));

            services.TryAddSingleton<KeycloakClient>();
            services.TryAddSingleton<ITokenProvider, TokenProvider>();

            configure?.Invoke(builder);
            return services;
        }

        internal static IServiceCollection AddKeycloakClient(
            this IServiceCollection services, Action<IHttpClientBuilder>? configure = null)
        {
            var builder = services
                .AddHttpClient<KeycloakClient>(
                    (serviceProvider, client) =>
                    {
                        var options = serviceProvider
                            .GetRequiredService<IOptions<ScanUploadProxyOptions>>()
                            .Value;
                        client.Timeout = options.KeycloakTimeout;
                    }
                )
                .SetHandlerLifetime(TimeSpan.FromMinutes(10));

            services.TryAddSingleton<KeycloakClient>();
            services.TryAddSingleton<ITokenProvider, TokenProvider>();

            configure?.Invoke(builder);

            return services;
        }
    }
}
