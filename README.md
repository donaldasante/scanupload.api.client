# ScanUpload.Api.Client – .NET Integration Guide

[ScanUpload](https://qa-app.scanupload.net/) enables the integration and the ability to use QR codes to scan and upload files directly from a mobile device to your webapp.
This guide explains how to integrate **ScanUpload.Api.Client** into a modern or legacy .NET application. The client library targets **.NET Standard 2.1**, making it compatible with:

-   .NET 6+
-   .NET 7
-   .NET 8
-   .NET 9 
-   .NET 10 (preview)
-   ASP.NET Core applications 
-   Older supported .NET runtimes that support .NET Standard 2.1

## Prerequisites

-   [.NET SDK](https://dotnet.microsoft.com/en-us/download)
-   [A ScanUpload account](https://qa-app.scanupload.net/)
-   A ScanUpload **Client ID** and **Client Secret**

```sh
dotnet new webapi -n ScanUploadExample
cd  ScanUploadExample
```

This works for:

-   .NET 6+
-   .NET 9 / 10 previews
-   Existing ASP.NET Core projects

## Install the ScanUpload API client

```sh
dotnet add package ScanUpload.Api.Client --version 0.1.0-alpha.7
dotnet add package Microsoft.Extensions.Http.Resilience
```

### Using Visual Studio

1.  Right‑click the project → **Manage NuGet Packages** 
2.  Enable **Include prerelease**   
3.  Search for **ScanUpload.Api.Client**  
4.  Install the latest  version   
5.  Install **Microsoft.Extensions.Http.Resilience** (optional)

## Configuration
Add the ScanUpload configuration section to `appsettings.json`:

```json
  "ScanUploadProxy": {
    "ScanUploadTargetBaseUrl": "https://qa-hub.scanupload.net/api/front-end",
    "ScanUploadRoutePrefix": "/scanupload-api",
    "ScanUploadStripRoutePrefix": true,
    "ScanUploadRequestTimeout": "00:01:30",
    "ScanUploadHeadersToForward": [
      "Authorization",
      "Content-Type",
      "User-Agent",
      "X-Requested-With",
      "X-API-Key"
    ],
    "ScanUploadApiClient": {
      "ScanUploadBaseUrl": "https://qa-hub.scanupload.net"
    },
    "ScanUploadAdditionalHeaders": {
      "X-Forwarded-By": "ScanUpload-Proxy",
      "X-Proxy-Version": "1.0"
    },
    "KeycloakServerUrl": "https://identity.scanupload.net/",
    "KeycloakRealm": "qa-scanupload-hub",
    "KeycloakScope": "openid profile email scanupload.hub"
  }
```

🔐 For local development, store secrets using **User Secrets** instead of committing them to source control.

### Configure user secrets
Please use ASP.NET Core user secrets for local development. These values are **not** committed to source control.

```sh
dotnet user-secrets init
```
Add your ScanUpload credentials:

```sh
dotnet user-secrets set "ScanUploadProxy:KeycloakClientId"  "your-client-id"  
dotnet user-secrets set "ScanUploadProxy:KeycloakClientSecret"  "your-client-secret"
```
This creates a `secrets.json` file in your local user profile, for example:

```json
{
  "ScanUploadProxy": {
    "KeycloakClientId": "your-client-id",
    "KeycloakClientSecret": "your-client-secret"
  }
}
```
# Configure services in `Program.cs`

## Register ScanUpload services

```csharp
builder.Services.Configure<ScanUploadProxyOptions>(
    builder.Configuration.GetSection("ScanUploadProxy")
);

builder.Services.AddScanUploadProxy(
    builder.Configuration.GetSection("ScanUploadProxy").Bind,
    builder =>
    {
        builder.AddStandardResilienceHandler();
    }
);

builder.Services.AddScanUploadApiClient(
    builder.Configuration,
    builder =>
    {
        builder.AddStandardResilienceHandler();
    }
);
```

## Enable the ScanUpload proxy middleware
Add the middleware after routing and before endpoints:
```csharp
app.UseScanUploadProxy();
```

## Minimal `Program.cs` example
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ScanUploadProxyOptions>(
    builder.Configuration.GetSection("ScanUploadProxy")
);

builder.Services.AddScanUploadProxy(
    builder.Configuration.GetSection("ScanUploadProxy").Bind,
    builder =>
    {
        builder.AddStandardResilienceHandler();
    }
);

builder.Services.AddScanUploadApiClient(
    builder.Configuration,
    builder =>
    {
        builder.AddStandardResilienceHandler();
    }
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseScanUploadProxy();

app.MapControllers();

app.Run();

```

## Compatibility notes

-   The client targets **.NET Standard 2.1**
-   Works with both **modern** and **older** ASP.NET Core applications
-   Safe to use in long‑term support (LTS) environments
-   Fully compatible with Docker and cloud deployments

## Resilience and reliability (Optional)

The client integrates with **Microsoft.Extensions.Http.Resilience**, providing:

-   Automatic retries  
-   Timeouts 
-   Circuit breakers
-   Transient fault handling
    
This ensures reliable communication with the ScanUpload API in production.