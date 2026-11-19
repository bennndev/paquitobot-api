using Microsoft.EntityFrameworkCore;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

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
}
