using PaquitoBot.Domain.Models;

namespace PaquitoBot.Domain.Repositories;

public interface IProgramCourseRepository : IGenericRepository<ProgramCourse>
{
   Task<bool> LinkExistsAsync(Guid programId, Guid courseId, CancellationToken cancellationToken = default);
}
