namespace PaquitoBot.Infrastructure.Services.Frappe;

public class FrappeApiException : Exception
{
    public int StatusCode { get; }

    public FrappeApiException(int statusCode, string? message, string? details = null)
        : base($"Frappe API returned HTTP {statusCode}: {message}"
            + (string.IsNullOrWhiteSpace(details) ? string.Empty : $". {details}"))
    {
        StatusCode = statusCode;
    }
}

public sealed class FrappeAuthenticationException : FrappeApiException
{
    public FrappeAuthenticationException(int statusCode, string? details)
        : base(statusCode, "Authentication failed", details)
    {
    }
}
