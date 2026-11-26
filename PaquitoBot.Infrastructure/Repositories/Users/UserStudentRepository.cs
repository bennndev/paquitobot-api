using Microsoft.EntityFrameworkCore;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.DbContext;

namespace PaquitoBot.Infrastructure.Repositories.Users;

public class UserStudentRepository : GenericRepository<UserStudent>, IUserStudentRepository
{
    public UserStudentRepository(PaquitoBotDbContext context) : base(context)
    {
    }

    public Task<bool> PairExistsAsync(Guid userId, Guid studentId, CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(
            l => l.UserId == userId && l.StudentId == studentId,
            cancellationToken);
    }

    public Task<Guid?> GetPrimaryStudentIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking()
            .Where(link => link.UserId == userId && link.IsPrimary)
            .Select(link => (Guid?)link.StudentId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<bool> UserHasPrimaryAsync(
        Guid userId,
        Guid? excludingLinkId = null,
        CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(
            l => l.UserId == userId
                && l.IsPrimary
                && (excludingLinkId == null || l.LinkId != excludingLinkId),
            cancellationToken);
    }

    protected override IOrderedQueryable<UserStudent> DefaultOrder(IQueryable<UserStudent> query)
    {
        return query.OrderBy(l => l.CreatedAt);
    }
}
