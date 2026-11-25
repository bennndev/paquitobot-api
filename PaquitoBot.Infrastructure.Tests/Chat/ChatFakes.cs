using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Tests.Chat;

internal class InMemoryRepository<T> : IGenericRepository<T> where T : class
{
    private static readonly System.Reflection.PropertyInfo IdProperty = typeof(T).GetProperty($"{typeof(T).Name}Id")!;

    public List<T> Items { get; } = [];

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Items.FirstOrDefault(item => (Guid)IdProperty.GetValue(item)! == id));
    }

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<T>>(Items.ToList());
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Items.Any(item => (Guid)IdProperty.GetValue(item)! == id));
    }

    public virtual Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if ((Guid)IdProperty.GetValue(entity)! == Guid.Empty)
        {
            IdProperty.SetValue(entity, Guid.NewGuid());
        }

        Items.Add(entity);

        return Task.CompletedTask;
    }

    public void Remove(T entity)
    {
        Items.Remove(entity);
    }

    public Task<T?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Items.AsQueryable().FirstOrDefault(predicate));
    }

    public Task<bool> AnyAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Items.AsQueryable().Any(predicate));
    }

    public Task<T> UpsertByFrappeExternalIdAsync(
        string externalId,
        Action<T> update,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}

internal class InMemoryConversationRepository : InMemoryRepository<Conversation>, IConversationRepository
{
    public Task<IReadOnlyList<Conversation>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Conversation>>(Items.Where(c => c.UserId == userId).ToList());
    }
}

internal class InMemoryMessageRepository : InMemoryRepository<Message>, IMessageRepository
{
    public override Task AddAsync(Message entity, CancellationToken cancellationToken = default)
    {
        if (entity.ConversationId == Guid.Empty && entity.Conversation is not null)
        {
            entity.ConversationId = entity.Conversation.ConversationId;
        }

        return base.AddAsync(entity, cancellationToken);
    }

    public Task<IReadOnlyList<Message>> GetByConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Message>>(Items.Where(m => m.ConversationId == conversationId).ToList());
    }
}

internal class FakeUnitOfWork : IUnitOfWork
{
    private readonly InMemoryConversationRepository _conversations = new();
    private readonly InMemoryMessageRepository _messages = new();

    public IConversationRepository Conversations => _conversations;
    public IMessageRepository Messages => _messages;

    public List<Conversation> StoredConversations => _conversations.Items;
    public List<Message> StoredMessages => _messages.Items;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);

    public void DiscardPendingChanges()
    {
    }

    public Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        return operation(cancellationToken);
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        if (typeof(TEntity) == typeof(Conversation))
        {
            return (IGenericRepository<TEntity>)(object)_conversations;
        }

        if (typeof(TEntity) == typeof(Message))
        {
            return (IGenericRepository<TEntity>)(object)_messages;
        }

        throw new NotSupportedException();
    }

    public IUserStudentRepository UserStudents => throw new NotSupportedException();
    public IUserRepository Users => throw new NotSupportedException();
    public IAssessmentResultRepository AssessmentResults => throw new NotSupportedException();
    public IAssessmentPlanRepository AssessmentPlans => throw new NotSupportedException();
    public IAssessmentRepository Assessments => throw new NotSupportedException();
    public IAttendanceRecordRepository AttendanceRecords => throw new NotSupportedException();
    public ISyncRunRepository SyncRuns => throw new NotSupportedException();
    public IKnowledgeDocumentRepository KnowledgeDocuments => throw new NotSupportedException();
}

internal class StubCurrentUser(Guid? userId) : ICurrentUser
{
    public Guid? UserId { get; } = userId;
}

internal class StubRetriever(IReadOnlyList<KnowledgeSource> sources, bool fails = false) : IKnowledgeRetriever
{
    public Task<IReadOnlyList<KnowledgeSource>> RetrieveAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        return fails
            ? throw new InvalidOperationException("retrieval failed")
            : Task.FromResult(sources);
    }
}

internal class StubAnswerProvider(bool fails = false) : IAnswerProvider
{
    public Task<string> GenerateAnswerAsync(
        string question,
        IReadOnlyList<KnowledgeSource> sources,
        CancellationToken cancellationToken = default)
    {
        return fails
            ? throw new InvalidOperationException("provider failed")
            : Task.FromResult($"answer to: {question}");
    }
}
