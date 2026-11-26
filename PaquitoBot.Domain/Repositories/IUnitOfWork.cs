namespace PaquitoBot.Domain.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    void DiscardPendingChanges();

    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    IUserStudentRepository UserStudents { get; }
    IUserRepository Users { get; }
    ICourseRepository Courses { get; }
    IConversationRepository Conversations { get; }
    IMessageRepository Messages { get; }
    IAssessmentResultRepository AssessmentResults { get; }
    IAssessmentPlanRepository AssessmentPlans { get; }
    IAssessmentRepository Assessments { get; }
    IAttendanceRecordRepository AttendanceRecords { get; }
    ISyncRunRepository SyncRuns { get; }
    IKnowledgeDocumentRepository KnowledgeDocuments { get; }
}
