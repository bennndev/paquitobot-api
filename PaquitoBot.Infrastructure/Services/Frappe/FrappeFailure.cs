namespace PaquitoBot.Infrastructure.Services.Frappe;

public enum FrappeFailure
{
    Authentication,
    NotFound,
    RateLimited,
    Unavailable,
    Timeout,
    InvalidResponse,
    RequestRejected
}
