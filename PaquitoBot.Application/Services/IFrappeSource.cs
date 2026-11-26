namespace PaquitoBot.Application.Services;

public interface IFrappeSource<TDto>
{
    Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
