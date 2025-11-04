using Microsoft.Extensions.DependencyInjection;
using ScanUpload.Api.Client.KeycloakIntegration;

// 1. Create service collection
var services = new ServiceCollection();

// 2. Configure KeycloakOptions
services.Configure<KeycloakOptions>(opts =>
{
    opts.ServerUrl = "https://identity.scanupload.net/";
    opts.Realm = "qa-scanupload-hub";
    opts.ClientId = "your-client-id";
    opts.ClientSecret = "your-client-secret";
    opts.Scope = "openid profile email scanupload.hub";
    opts.EarlyRefreshSeconds = 30;
});

services.AddHttpClient<KeycloakClient>().SetHandlerLifetime(TimeSpan.FromMinutes(5));

// 3. Register KeycloakClient
services.AddTransient<KeycloakClient>();

// 4. Build provider
var provider = services.BuildServiceProvider();

// 5. Resolve and use KeycloakClient
var client = provider.GetRequiredService<KeycloakClient>();

try
{
    var tokenResponse = await client.GetClientCredentialsTokenAsync();
    Console.WriteLine($"Access Token: {tokenResponse.AccessToken}");
    Console.WriteLine($"Expires In: {tokenResponse.ExpiresIn} seconds");
}
catch (KeycloakException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
