using Microsoft.EntityFrameworkCore;
using PaquitoBot.Domain.Models;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Infrastructure.DbContext;

namespace PaquitoBot.Infrastructure.Repositories.Academic;

internal sealed class ProgramEnrollmentRepository(PaquitoBotDbContext context)
    : GenericRepository<ProgramEnrollment>(context), IProgramEnrollmentRepository
{
    public Task<ProgramEnrollment?> FindEnrollmentAsync(Guid studentId, Guid programId, Guid periodId, CancellationToken cancellationToken = default)
        => DbSet.FirstOrDefaultAsync(enrollment => enrollment.StudentId == studentId && enrollment.ProgramId == programId && enrollment.AcademicPeriodId == periodId, cancellationToken);
}
