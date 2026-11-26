using PaquitoBot.Domain.Models;

namespace PaquitoBot.Domain.Repositories;

public interface IProgramEnrollmentRepository : IGenericRepository<ProgramEnrollment>
{
   Task<ProgramEnrollment?> FindEnrollmentAsync(Guid studentId, Guid programId, Guid periodId, CancellationToken cancellationToken = default);
}
