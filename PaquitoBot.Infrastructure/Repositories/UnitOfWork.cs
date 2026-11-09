using Microsoft.EntityFrameworkCore;
using Npgsql;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly PaquitoBotDbContext _context;

    public UnitOfWork(PaquitoBotDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresException)
        {
            if (postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ConflictException("A record with the same unique value already exists.");
            }

            if (postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                throw new ConflictException("The operation conflicts with related records.");
            }

            throw;
        }
    }
}
