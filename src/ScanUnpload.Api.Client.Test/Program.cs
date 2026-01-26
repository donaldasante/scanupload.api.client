using ScanUpload.Api.Client.ApiClient;
using ScanUpload.Api.Client.Extensions;
using ScanUpload.Api.Client.Interface;
using ScanUpload.Api.Client.KeycloakIntegration;
using ScanUpload.Api.Client.Middleware;
using ScanUpload.Api.Client.Proxy;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Use scanupload proxy
builder.Services.Configure<ScanUploadProxyOptions>(
    builder.Configuration.GetSection("ScanUploadProxy")
);
builder.Services.AddScanUploadProxy(builder.Configuration.GetSection("ScanUploadProxy").Bind);
builder.Services.AddTransient<AuthenticatedHttpClientHandler>();
builder
  .Services.AddHttpClient<IScanUploadApiClient, ScanUploadApiClient>(client =>
  {
      var apiUrl = builder.Configuration["ScanUploadProxy:ScanUploadApiClient:ScanUploadBaseUrl"]
        ?? throw new FileNotFoundException("ScanUpload download URL not found");
      client.BaseAddress = new Uri(apiUrl);
      client.DefaultRequestHeaders.Add("Accept", "application/json");
      client.Timeout = TimeSpan.FromSeconds(120);
  })
  .AddHttpMessageHandler<AuthenticatedHttpClientHandler>();

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
                using var file = File.Create(Path.Combine("output", fileName)); 
                await stream.CopyToAsync(file, cancellationToken);
                return Results.Ok(file);
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
