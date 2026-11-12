using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class KnowledgeDocumentRepository : GenericRepository<KnowledgeDocument>, IKnowledgeDocumentRepository
{
    private const string LikeEscape = "\\";

    public KnowledgeDocumentRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<KnowledgeDocument>> SearchAsync(
        Guid? courseId,
        string? status,
        string? titleText,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();

        if (courseId.HasValue)
        {
            query = query.Where(d => d.CourseId == courseId);
        }

        if (status is not null)
        {
            query = query.Where(d => d.Status == status);
        }

        if (titleText is not null)
        {
            var pattern = $"%{EscapeLike(titleText)}%";
            query = query.Where(d => EF.Functions.ILike(d.Title, pattern, LikeEscape));
        }

        return await query
            .OrderByDescending(d => d.UpdatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    protected override IOrderedQueryable<KnowledgeDocument> DefaultOrder(IQueryable<KnowledgeDocument> query)
    {
        return query.OrderByDescending(d => d.UpdatedAt);
    }

    private static string EscapeLike(string value)
    {
        return value
            .Replace(LikeEscape, LikeEscape + LikeEscape)
            .Replace("%", LikeEscape + "%")
            .Replace("_", LikeEscape + "_");
    }
}
