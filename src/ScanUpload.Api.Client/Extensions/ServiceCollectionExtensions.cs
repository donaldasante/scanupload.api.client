using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScanUpload.Api.Client.Interface;
using ScanUpload.Api.Client.KeycloakIntegration;

namespace ScanUpload.Api.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKeycloakClient(
            this IServiceCollection services,
            Action<KeycloakOptions> configureOptions
        )
        {
            services.Configure(configureOptions);
            services
                .AddHttpClient<KeycloakClient>(
                    (serviceProvider, client) =>
                    {
                        var options = serviceProvider
                            .GetRequiredService<IOptions<KeycloakOptions>>()
                            .Value;
                        client.Timeout = options.Timeout;
                    }
                )
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.AddSingleton<KeycloakClient>();
            services.AddSingleton<ITokenProvider, TokenProvider>();

            return services;
        }

        public static IServiceCollection AddKeycloakClient(
            this IServiceCollection services,
            KeycloakOptions options
        )
        {
            services.AddSingleton(Options.Create(options));
            services
                .AddHttpClient<KeycloakClient>(client =>
                {
                    client.Timeout = options.Timeout;
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));
            services.AddSingleton<KeycloakClient>();
            services.AddSingleton<ITokenProvider, TokenProvider>();

            return services;
        }
    }
}
