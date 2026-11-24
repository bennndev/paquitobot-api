namespace PaquitoBot.Application.Integrations.Frappe;

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
