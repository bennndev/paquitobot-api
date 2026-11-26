using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Application.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public sealed class FrappeHealthCheck(IFrappeClient client) : IHealthCheck
{
    public const string Name = "frappe";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await client.CheckHealthAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (FrappeException exception)
        {
            return HealthCheckResult.Unhealthy(exception.Message);
        }
    }
}
