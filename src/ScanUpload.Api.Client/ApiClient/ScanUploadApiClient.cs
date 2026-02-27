using ScanUpload.Api.Client.Interface;
using System.IO.Compression;

namespace ScanUpload.Api.Client.ApiClient
{
    public sealed class ScanUploadApiClient : IScanUploadApiClient
    {
        private readonly HttpClient _httpClient;
        public ScanUploadApiClient(HttpClient httpClient) { _httpClient = httpClient; }

        public async Task DownloadAsync(string sessionId, Func<string, Stream, CancellationToken, Task> processEntry, CancellationToken cancellationToken = default)
        {
            var requestUri = new Uri($"/api/file-management/download-session/{sessionId}", UriKind.Relative);
            var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync();
            using var zip = new ZipArchive(responseStream, ZipArchiveMode.Read, leaveOpen: false);

            foreach (var entry in zip.Entries)
            {
                using var entryStream = entry.Open();
                await processEntry(entry.FullName, entryStream, cancellationToken);
            }
        }
    }
}
