using System.Collections;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private Hashtable? _repositories;
    private readonly PaquitoBotDbContext _context;
    public IUserStudentRepository UserStudents { get; }

    public UnitOfWork(PaquitoBotDbContext context, IUserStudentRepository userStudents)
    {
        _context = context;
        UserStudents = userStudents;
        _repositories = new Hashtable();
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
    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity).Name;

        if (_repositories.ContainsKey(type))
        {
            return (IGenericRepository<TEntity>)_repositories[type];
        }

        var repositoryType = typeof(GenericRepository<>);
        var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context);

        if (repositoryInstance != null)
        {
            _repositories.Add(type, repositoryInstance);
            return (IGenericRepository<TEntity>)repositoryInstance;
        }
        throw new Exception($"Could not create the repository instance for the type {type}");
    }
}
