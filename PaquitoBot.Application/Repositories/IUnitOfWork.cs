namespace PaquitoBot.Application.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Drops every pending change so a failed save cannot leak into the next operation.</summary>
    void DiscardPendingChanges();

    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    IUserStudentRepository UserStudents { get; }
    IUserRepository Users { get; }
    IMessageRepository Messages { get; }
    IAssessmentResultRepository AssessmentResults { get; }
    IAssessmentPlanRepository AssessmentPlans { get; }
    IAssessmentRepository Assessments { get; }
    IAttendanceRecordRepository AttendanceRecords { get; }
    ISyncRunRepository SyncRuns { get; }
    IKnowledgeDocumentRepository KnowledgeDocuments { get; }
}
