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
}
