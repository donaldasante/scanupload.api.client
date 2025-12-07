namespace ScanUpload.Api.Client.Proxy
{
    public sealed class ScanUploadProxyException : Exception
    {
        public string? ErrorCode { get; }
        public int? StatusCode { get; }

        public ScanUploadProxyException(string message)
            : base(message) { }

        public ScanUploadProxyException(string message, Exception innerException)
            : base(message, innerException) { }

        public ScanUploadProxyException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public ScanUploadProxyException(string errorCode, string message, int statusCode)
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public ScanUploadProxyException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
