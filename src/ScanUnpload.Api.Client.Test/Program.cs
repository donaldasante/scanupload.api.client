using ScanUpload.Api.Client.Extensions;
using ScanUpload.Api.Client.Interface;
using ScanUpload.Api.Client.KeycloakIntegration;
using ScanUpload.Api.Client.Middleware;
using ScanUpload.Api.Client.Proxy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Use scanupload proxy
builder.Services.Configure<ScanUploadProxyOptions>(
    builder.Configuration.GetSection("ScanUploadProxy")
);
builder.Services.AddScanUploadProxy(builder.Configuration.GetSection("ScanUploadProxy").Bind, builder =>
{
    builder.AddStandardResilienceHandler();
});

builder.Services.AddScanUploadApiClient(builder.Configuration, builder =>
{
    builder.AddStandardResilienceHandler();
});

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

app.MapGet(
    "/download-file/{sessionId}",
    async (string sessionId,IScanUploadApiClient scanUploadApiClient, CancellationToken cancellationToken) =>
    {
        try
        {
            await foreach (var (fileName, stream) in scanUploadApiClient.DownloadAsync(sessionId, cancellationToken)) 
            {
                Console.WriteLine($"Received file: {fileName}");
                var outputDir = Path.Combine(Environment.CurrentDirectory, "output");
                Directory.CreateDirectory(outputDir);

                using var file = File.Create(Path.Combine(outputDir, fileName));
                await stream.CopyToAsync(file, cancellationToken);
            }

            return Results.Ok();
        }
        catch (KeycloakException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
);

app.Run();
