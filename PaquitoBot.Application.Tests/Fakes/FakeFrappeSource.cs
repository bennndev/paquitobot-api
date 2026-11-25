using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.Tests.Fakes;

public class FakeFrappeSource<TDto> : IFrappeSource<TDto>
{
    public List<TDto> Items { get; } = [];

    public Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TDto>>(Items.ToList());
}
