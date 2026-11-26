namespace PaquitoBot.Infrastructure.Services.Frappe;

public sealed class FrappeException(FrappeFailure failure) : Exception(GetMessage(failure))
{
    public FrappeFailure Failure { get; } = failure;

    private static string GetMessage(FrappeFailure failure) => failure switch
    {
        FrappeFailure.Authentication => "Frappe rejected the integration credentials or permissions.",
        FrappeFailure.NotFound => "The requested Frappe resource was not found.",
        FrappeFailure.RateLimited => "Frappe is rate limiting requests.",
        FrappeFailure.Unavailable => "Frappe is unavailable.",
        FrappeFailure.Timeout => "The Frappe request timed out.",
        FrappeFailure.InvalidResponse => "Frappe returned an invalid response.",
        _ => "Frappe rejected the request."
    };
}
