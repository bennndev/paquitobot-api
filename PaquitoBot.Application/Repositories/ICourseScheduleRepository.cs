using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface ICourseScheduleRepository : IGenericRepository<CourseSchedule>
{
    Task<IReadOnlyList<CourseSchedule>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken = default);
}
