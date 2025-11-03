using ScanUpload.Api.Client.KeycloakIntegration;

var options = new KeycloakOptions
{
    ServerUrl = "https://identity.scanupload.net/",
    Realm = "qa-scanupload-hub",
    ClientId = "",
    ClientSecret = "",
    Scope = "openid profile email scanupload.hub",
};

using var client = new KeycloakClient(options);

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
