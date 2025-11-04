using ScanUpload.Api.Client.Extensions;
using ScanUpload.Api.Client.Interface;
using ScanUpload.Api.Client.KeycloakIntegration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Keycloak client
builder.Services.AddKeycloakClient(options =>
{
    options.ServerUrl = builder.Configuration["Keycloak:ServerUrl"] ?? "";
    options.Realm = builder.Configuration["Keycloak:Realm"] ?? "";
    options.ClientId = builder.Configuration["Keycloak:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"] ?? "";
    options.Scope = builder.Configuration["Keycloak:Scope"] ?? "";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet(
    "/token",
    async (KeycloakClient keycloakClient) =>
    {
        try
        {
            var token = await keycloakClient.GetClientCredentialsTokenAsync();
            return Results.Ok(new { token.AccessToken, token.ExpiresIn });
        }
        catch (KeycloakException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
);

app.MapGet(
    "/cached-token",
    async (ITokenProvider provider) =>
    {
        try
        {
            var token = await provider.GetAccessTokenAsync();
            return Results.Ok(new { token.AccessToken, token.ExpiresIn });
        }
        catch (KeycloakException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
);

app.Run();
