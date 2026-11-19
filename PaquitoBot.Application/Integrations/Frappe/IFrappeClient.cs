namespace PaquitoBot.Application.Integrations.Frappe;

public interface IFrappeClient
{
    Task<FrappePage<T>> GetPageAsync<T>(FrappeQuery query, CancellationToken cancellationToken = default)
        where T : notnull;

    Task CheckHealthAsync(CancellationToken cancellationToken = default);
}
