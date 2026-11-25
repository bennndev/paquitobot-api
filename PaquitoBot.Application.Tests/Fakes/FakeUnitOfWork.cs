using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.Tests.Fakes;

public class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public int DiscardCount { get; private set; }

    public void DiscardPendingChanges() => DiscardCount++;

    public Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
        => operation(cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(0);
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class => throw new NotSupportedException();

    public IUserStudentRepository UserStudents => throw new NotSupportedException();

    public IUserRepository Users => throw new NotSupportedException();

    public IMessageRepository Messages => throw new NotSupportedException();

    public IAssessmentResultRepository AssessmentResults => throw new NotSupportedException();

    public IAssessmentPlanRepository AssessmentPlans => throw new NotSupportedException();

    public IAssessmentRepository Assessments => throw new NotSupportedException();

    public IAttendanceRecordRepository AttendanceRecords => throw new NotSupportedException();

    public ISyncRunRepository SyncRuns => throw new NotSupportedException();

    public IKnowledgeDocumentRepository KnowledgeDocuments => throw new NotSupportedException();
}
