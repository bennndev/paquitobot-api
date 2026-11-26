using PaquitoBot.Application.Dtos.Frappe;
namespace PaquitoBot.Application.Services;

public interface IFrappeClient
{
    Task<FrappePage<T>> GetPageAsync<T>(FrappeQuery query, CancellationToken cancellationToken = default)
        where T : notnull;

    Task CheckHealthAsync(CancellationToken cancellationToken = default);
}
