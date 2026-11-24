using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class FrappeSyncedRepository<T> : GenericRepository<T>, IFrappeSyncedRepository<T>
    where T : class, IFrappeSynced
{
    private const string ExternalIdProperty = nameof(IFrappeSynced.FrappeExternalId);

    public FrappeSyncedRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public Task<T?> GetByFrappeExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(
            e => EF.Property<string>(e, ExternalIdProperty) == externalId,
            cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, T>> GetByFrappeExternalIdsAsync(
        IEnumerable<string> externalIds,
        CancellationToken cancellationToken = default)
    {
        var ids = externalIds.Distinct().ToList();

        var entities = await DbSet
            .Where(e => ids.Contains(EF.Property<string>(e, ExternalIdProperty)))
            .ToListAsync(cancellationToken);

        return entities.ToDictionary(e => e.FrappeExternalId!);
    }
}
