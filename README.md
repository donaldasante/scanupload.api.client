# ScanUpload.Api.Client

ScanUpload.Api.Client enables .NET backend applications to download files uploaded through a ScanUpload session.

## Backend downloads

Use this package from an ASP.NET Core application, worker service, console application, or other trusted backend. It obtains an access token and downloads a session file bundle so that your backend can store or process the files.

Do not use this package in a browser, mobile application, or other public client. Downloads must occur on the backend.

## Client secret required

The API client uses OAuth 2.0 client-credentials authentication. A ScanUpload OAuth client ID and client secret are required to obtain the bearer token used for downloads.

Keep the client secret private. Use user secrets for local development and an environment variable or managed secret store in production. Never commit the secret to source control or expose it to users.

## Requirements

- .NET 8, .NET 9, or .NET 10
- A ScanUpload account
- A ScanUpload OAuth client ID and client secret

## Install

```sh
dotnet add package ScanUpload.Api.Client
```

## Download example

Register the API client with the application's configuration:

```csharp
using ScanUpload.Api.Client.Extensions;

builder.Services.AddScanUploadApiClient(builder.Configuration);
```

The following backend endpoint, based on `ScanUpload.Api.Client.Test`, downloads a
session and writes every received file to an `output` directory:

```csharp
using ScanUpload.Api.Client.ApiClient;

app.MapGet("/download-file/{sessionId}", async (
	string sessionId,
	IScanUploadApiClient client,
	CancellationToken ct) =>
{
	var outputDir = Path.Combine(Environment.CurrentDirectory, "output");
	Directory.CreateDirectory(outputDir);

	await client.DownloadAsync(sessionId, async (fileName, stream, cancellation) =>
	{
		Console.WriteLine($"Received file: {fileName}");
		await using var file = File.Create(Path.Combine(outputDir, fileName));
		await stream.CopyToAsync(file, cancellation);
	}, ct);

	return Results.Ok(new { savedTo = outputDir });
});
```

`DownloadAsync` streams the downloaded archive and invokes the callback for each file.
The backend can save each file locally, send it to blob storage, or process it directly.
