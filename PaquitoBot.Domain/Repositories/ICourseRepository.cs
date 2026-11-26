using PaquitoBot.Domain.Models;

namespace PaquitoBot.Domain.Repositories;

public interface ICourseRepository : IGenericRepository<Course>
{
    Task<IReadOnlyList<Course>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
}
