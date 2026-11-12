namespace PaquitoBot.Application.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    IUserStudentRepository UserStudents { get; }
    ISyncRunRepository SyncRuns { get; }
    IKnowledgeDocumentRepository KnowledgeDocuments { get; }
}
