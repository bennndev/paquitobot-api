namespace PaquitoBot.Application.Services.Sync;

public interface IFrappeSource<TDto>
{
    Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
