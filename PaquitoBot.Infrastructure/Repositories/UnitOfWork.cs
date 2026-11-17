using System.Collections;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly Hashtable _repositories = new();
    private readonly PaquitoBotDbContext _context;

    public IUserStudentRepository UserStudents { get; }
    public IUserRepository Users { get; }
    public IMessageRepository Messages { get; }
    public IAssessmentResultRepository AssessmentResults { get; }
    public IAssessmentPlanRepository AssessmentPlans { get; }
    public IAssessmentRepository Assessments { get; }
    public IAttendanceRecordRepository AttendanceRecords { get; }
    public ISyncRunRepository SyncRuns { get; }
    public IKnowledgeDocumentRepository KnowledgeDocuments { get; }

    public UnitOfWork(
        PaquitoBotDbContext context,
        IUserStudentRepository userStudents,
        IUserRepository users,
        IMessageRepository messages,
        IAssessmentResultRepository assessmentResults,
        IAssessmentPlanRepository assessmentPlans,
        IAssessmentRepository assessments,
        IAttendanceRecordRepository attendanceRecords,
        ISyncRunRepository syncRuns,
        IKnowledgeDocumentRepository knowledgeDocuments)
    {
        _context = context;
        UserStudents = userStudents;
        Users = users;
        Messages = messages;
        AssessmentResults = assessmentResults;
        AssessmentPlans = assessmentPlans;
        Assessments = assessments;
        AttendanceRecords = attendanceRecords;
        SyncRuns = syncRuns;
        KnowledgeDocuments = knowledgeDocuments;
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
            return (IGenericRepository<TEntity>)_repositories[type]!;
        }

        var repositoryInstance = Activator.CreateInstance(
            typeof(GenericRepository<>).MakeGenericType(typeof(TEntity)),
            _context);

        if (repositoryInstance is null)
        {
            throw new InvalidOperationException($"Could not create the repository instance for the type {type}.");
        }

        _repositories.Add(type, repositoryInstance);

        return (IGenericRepository<TEntity>)repositoryInstance;
    }
}
