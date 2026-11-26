using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface ICourseRepository : IGenericRepository<Course>
{
    Task<IReadOnlyList<Course>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
}
