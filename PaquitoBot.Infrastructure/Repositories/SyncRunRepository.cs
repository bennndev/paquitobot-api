using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class SyncRunRepository : GenericRepository<SyncRun>, ISyncRunRepository
{
    public SyncRunRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<SyncRun>> GetFilteredAsync(
        string? entityName,
        string? status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(s => entityName == null || s.EntityName == entityName)
            .Where(s => status == null || s.Status == status)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SyncRun>> GetLatestByEntityAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .GroupBy(s => s.EntityName)
            .Select(g => g.OrderByDescending(s => s.StartDate).First())
            .ToListAsync(cancellationToken);
    }

    protected override IOrderedQueryable<SyncRun> DefaultOrder(IQueryable<SyncRun> query)
    {
        return query.OrderByDescending(s => s.StartDate);
    }
}
