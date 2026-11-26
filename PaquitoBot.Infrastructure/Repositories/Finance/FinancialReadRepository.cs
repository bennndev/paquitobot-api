using Microsoft.EntityFrameworkCore;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.DbContext;

namespace PaquitoBot.Infrastructure.Repositories.Finance;

public sealed class FinancialReadRepository(PaquitoBotDbContext context) : IFinancialReadRepository
{
    public Task<bool> StudentExistsAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        context.Students.AsNoTracking()
            .AnyAsync(student => student.StudentId == studentId, cancellationToken);

    public Task<bool> CanUserAccessStudentAsync(
        Guid userId, Guid studentId, CancellationToken cancellationToken = default) =>
        context.Users.AsNoTracking().AnyAsync(user =>
            user.UserId == userId && user.IsActive &&
            (user.Role.ToLower() == "admin" ||
             context.UserStudents.Any(link => link.UserId == userId && link.StudentId == studentId)),
            cancellationToken);

    public async Task<IReadOnlyList<TuitionFee>> GetTuitionFeesAsync(
        Guid studentId, CancellationToken cancellationToken = default) =>
        await context.TuitionFees.AsNoTracking()
            .Where(fee => fee.StudentId == studentId)
            .OrderBy(fee => fee.DueDate)
            .ThenBy(fee => fee.TuitionFeeId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetPaymentsAsync(
        Guid studentId, CancellationToken cancellationToken = default) =>
        await context.Payments.AsNoTracking()
            .Where(payment => payment.StudentId == studentId)
            .OrderBy(payment => payment.PaidOn)
            .ThenBy(payment => payment.PaymentId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetFilteredPaymentsAsync(
        Guid studentId, string? status, DateOnly? from, DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var query = context.Payments.AsNoTracking()
            .Where(payment => payment.StudentId == studentId);

        if (status is not null)
        {
            var normalizedStatus = status.Trim().ToLowerInvariant();
            query = query.Where(payment => payment.Status != null &&
                payment.Status.ToLower() == normalizedStatus);
        }

        if (from.HasValue)
        {
            query = query.Where(payment => payment.PaidOn.HasValue &&
                payment.PaidOn.Value >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(payment => payment.PaidOn.HasValue &&
                payment.PaidOn.Value <= to.Value);
        }

        return await query.OrderByDescending(payment => payment.PaidOn)
            .ThenBy(payment => payment.PaymentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProgramEnrollment>> GetProgramEnrollmentsAsync(
        Guid studentId, Guid? programId, Guid? academicPeriodId,
        CancellationToken cancellationToken = default)
    {
        var query = context.ProgramEnrollments.AsNoTracking()
            .Include(enrollment => enrollment.Program)
            .Where(enrollment => enrollment.StudentId == studentId);

        if (programId.HasValue)
            query = query.Where(enrollment => enrollment.ProgramId == programId.Value);

        if (academicPeriodId.HasValue)
            query = query.Where(enrollment => enrollment.AcademicPeriodId == academicPeriodId.Value);

        return await query.OrderBy(enrollment => enrollment.Program.Name)
            .ThenBy(enrollment => enrollment.AcademicPeriodId)
            .ThenBy(enrollment => enrollment.ProgramEnrollmentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FeeStructure>> GetFeeStructuresForProgramsAsync(
        IReadOnlyCollection<Guid> programIds,
        CancellationToken cancellationToken = default)
    {
        if (programIds.Count == 0) return Array.Empty<FeeStructure>();

        var ids = programIds.ToArray();
        return await context.FeeStructures.AsNoTracking()
            .Where(structure => structure.ProgramId.HasValue &&
                ids.Contains(structure.ProgramId.Value))
            .OrderBy(structure => structure.Name)
            .ThenBy(structure => structure.FeeStructureId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FeeSchedule>> GetFeeSchedulesForStructuresAsync(
        IReadOnlyCollection<Guid> feeStructureIds, Guid? academicPeriodId,
        CancellationToken cancellationToken = default)
    {
        if (feeStructureIds.Count == 0) return Array.Empty<FeeSchedule>();

        var ids = feeStructureIds.ToArray();
        var query = context.FeeSchedules.AsNoTracking()
            .Where(schedule => ids.Contains(schedule.FeeStructureId));

        if (academicPeriodId.HasValue)
        {
            query = query.Where(schedule =>
                !schedule.AcademicPeriodId.HasValue ||
                schedule.AcademicPeriodId == academicPeriodId.Value);
        }

        return await query.OrderBy(schedule => schedule.DueDate)
            .ThenBy(schedule => schedule.FeeScheduleId)
            .ToListAsync(cancellationToken);
    }
}
