using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

public interface IFinancialReadRepository
{
    Task<bool> StudentExistsAsync(Guid studentId, CancellationToken cancellationToken = default);

    Task<bool> CanUserAccessStudentAsync(
        Guid userId, Guid studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TuitionFee>> GetTuitionFeesAsync(
        Guid studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> GetPaymentsAsync(
        Guid studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> GetFilteredPaymentsAsync(
        Guid studentId, string? status, DateOnly? from, DateOnly? to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProgramEnrollment>> GetProgramEnrollmentsAsync(
        Guid studentId, Guid? programId, Guid? academicPeriodId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FeeStructure>> GetFeeStructuresForProgramsAsync(
        IReadOnlyCollection<Guid> programIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FeeSchedule>> GetFeeSchedulesForStructuresAsync(
        IReadOnlyCollection<Guid> feeStructureIds, Guid? academicPeriodId,
        CancellationToken cancellationToken = default);
}
