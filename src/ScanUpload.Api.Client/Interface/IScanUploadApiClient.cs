namespace ScanUpload.Api.Client.Interface
{
    public interface IScanUploadApiClient
    {
        Task DownloadAsync(string sessionId, Func<string, Stream, CancellationToken, Task> processEntry, CancellationToken cancellationToken = default);
    }
}
