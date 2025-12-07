namespace ScanUpload.Api.Client.KeycloakIntegration
{
    public sealed class KeycloakException : Exception
    {
        public string? ErrorCode { get; }
        public int? StatusCode { get; }

        public KeycloakException(string message)
            : base(message) { }

        public KeycloakException(string message, Exception innerException)
            : base(message, innerException) { }

        public KeycloakException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public KeycloakException(string errorCode, string message, int statusCode)
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public KeycloakException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
