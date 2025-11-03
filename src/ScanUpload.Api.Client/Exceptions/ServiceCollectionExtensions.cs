using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScanUpload.Api.Client.KeycloakIntegration;

namespace ScanUpload.Api.Client.Exceptions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKeycloakClient(
            this IServiceCollection services,
            Action<KeycloakOptions> configureOptions
        )
        {
            services.Configure(configureOptions);
            services.AddHttpClient<KeycloakClient>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider
                        .GetRequiredService<IOptions<KeycloakOptions>>()
                        .Value;
                    client.Timeout = options.Timeout;
                }
            );

            return services;
        }

        public static IServiceCollection AddKeycloakClient(
            this IServiceCollection services,
            KeycloakOptions options
        )
        {
            services.AddSingleton(Options.Create(options));
            services.AddHttpClient<KeycloakClient>(client =>
            {
                client.Timeout = options.Timeout;
            });

            return services;
        }
    }
}
