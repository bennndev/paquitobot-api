using Microsoft.Extensions.Diagnostics.HealthChecks;
using PaquitoBot.Application.Integrations.Frappe;

namespace PaquitoBot.Api.HealthChecks;

internal sealed class FrappeHealthCheck(IFrappeClient client) : IHealthCheck
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
