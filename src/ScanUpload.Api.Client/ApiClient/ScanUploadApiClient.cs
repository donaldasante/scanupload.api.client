
using ScanUpload.Api.Client.Interface;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace ScanUpload.Api.Client.ApiClient
{
    public sealed class ScanUploadApiClient : IScanUploadApiClient
    {
        private readonly HttpClient _httpClient; 
        public ScanUploadApiClient(HttpClient httpClient) { _httpClient = httpClient; }

        public async IAsyncEnumerable<(string FileName, Stream Content)> DownloadAsync(string sessionId, [EnumeratorCancellation]  CancellationToken cancellationToken = default)
        {
            var requestUri = new Uri($"/api/file-management/download-session/{sessionId}", UriKind.Relative);
            var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync();
            using var zip = new ZipArchive(responseStream, ZipArchiveMode.Read, leaveOpen: false);

            foreach (var entry in zip.Entries)
            {
                var entryStream = entry.Open();

                // You probably want to copy it into a MemoryStream or return the raw stream. 
                // Returning raw stream keeps everything streaming.
                
                yield return (entry.FullName, entryStream);
            }
        }
    }
}
