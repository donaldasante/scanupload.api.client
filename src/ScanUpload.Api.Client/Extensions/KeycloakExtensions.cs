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
            Action<ScanUploadProxyOptions> configureOptions
        )
        {
            services.Configure(configureOptions);
            services
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

            return services;
        }

        internal static IServiceCollection AddKeycloakClient(this IServiceCollection services)
        {
            services
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

            return services;
        }
    }
}
