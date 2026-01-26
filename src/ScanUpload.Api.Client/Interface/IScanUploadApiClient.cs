namespace ScanUpload.Api.Client.Interface
{
    public interface IScanUploadApiClient
    {
        IAsyncEnumerable<(string FileName, Stream Content)> DownloadAsync(string sessionId, CancellationToken cancellationToken = default);
    }
}
