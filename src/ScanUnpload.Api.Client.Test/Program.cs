using ScanUpload.Api.Client.Extensions;
using ScanUpload.Api.Client.Interface;
using ScanUpload.Api.Client.KeycloakIntegration;
using ScanUpload.Api.Client.Proxy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Use scanupload proxy
builder.Services.Configure<ScanUploadProxyOptions>(
    builder.Configuration.GetSection("ScanUploadProxy")
);
builder.Services.AddScanUploadProxy(builder.Configuration.GetSection("ScanUploadProxy").Bind);

var app = builder.Build();

app.UseScanUploadProxy();

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
