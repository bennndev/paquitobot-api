using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly PaquitoBotDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(PaquitoBotDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    private string KeyName => Context.Model.FindEntityType(typeof(T))!.FindPrimaryKey()!.Properties.Single().Name;

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DefaultOrder(DbSet.AsNoTracking()).ToListAsync(cancellationToken);
    }

    public virtual Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(e => EF.Property<Guid>(e, KeyName) == id, cancellationToken);
    }

    public virtual Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(predicate, cancellationToken);

    public virtual async Task<T> UpsertByFrappeExternalIdAsync(string externalId, Action<T> update,
        CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FirstOrDefaultAsync(
            row => EF.Property<string>(row, "FrappeExternalId") == externalId, cancellationToken);
        if (entity is null)
        {
            entity = Activator.CreateInstance<T>();
            await DbSet.AddAsync(entity, cancellationToken);
            Context.Entry(entity).Property("FrappeExternalId").CurrentValue = externalId;
        }

        update(entity);
        return entity;
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    protected virtual IOrderedQueryable<T> DefaultOrder(IQueryable<T> query)
    {
        return query.OrderBy(e => EF.Property<Guid>(e, KeyName));
    }
}
