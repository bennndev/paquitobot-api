namespace PaquitoBot.Application.Services.Sync;

/// <summary>
/// Read-only port to fetch every record of one Frappe document type as its external DTO.
/// Implementations must never expose credentials in exceptions or logs.
/// </summary>
public interface IFrappeSource<TDto>
{
    Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
