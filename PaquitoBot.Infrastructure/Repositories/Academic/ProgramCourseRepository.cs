using Microsoft.EntityFrameworkCore;
using PaquitoBot.Domain.Models;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Infrastructure.DbContext;

namespace PaquitoBot.Infrastructure.Repositories.Academic;

internal sealed class ProgramCourseRepository(PaquitoBotDbContext context)
    : GenericRepository<ProgramCourse>(context), IProgramCourseRepository
{
    public Task<bool> LinkExistsAsync(Guid programId, Guid courseId, CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(link => link.ProgramId == programId && link.CourseId == courseId, cancellationToken);
}
