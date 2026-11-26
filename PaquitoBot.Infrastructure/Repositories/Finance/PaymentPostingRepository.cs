using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.DbContext;

namespace PaquitoBot.Infrastructure.Repositories.Finance;

public sealed class PaymentPostingRepository(PaquitoBotDbContext context) : IPaymentPostingRepository
{
    private static readonly string[] ConfirmedStatuses =
    [
        "paid", "completed", "confirmed", "settled", "successful", "success"
    ];

    public async Task<Payment> RecordAsync(
        Payment candidate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);

            var existing = await context.Payments.AsNoTracking()
                .SingleOrDefaultAsync(payment =>
                    payment.FrappeExternalId == candidate.FrappeExternalId,
                    cancellationToken);

            if (existing is not null)
            {
                if (existing.StudentId != candidate.StudentId ||
                    existing.TuitionFeeId != candidate.TuitionFeeId ||
                    existing.Amount != candidate.Amount ||
                    !string.Equals(existing.CurrencyCode, candidate.CurrencyCode,
                        StringComparison.OrdinalIgnoreCase) ||
                    !ConfirmedStatuses.Contains(existing.Status?.ToLowerInvariant() ?? string.Empty) ||
                    (candidate.PaidOn.HasValue && existing.PaidOn != candidate.PaidOn) ||
                    !string.Equals(existing.PaymentMethod, candidate.PaymentMethod,
                        StringComparison.Ordinal) ||
                    !string.Equals(existing.Reference, candidate.Reference,
                        StringComparison.Ordinal))
                {
                    throw new ConflictException(
                        "This external payment identifier was already used for another payment.");
                }

                await transaction.CommitAsync(cancellationToken);
                return existing;
            }

            candidate.PaidOn ??= DateOnly.FromDateTime(DateTime.UtcNow);

            var studentExists = await context.Students.AsNoTracking()
                .AnyAsync(s => s.StudentId == candidate.StudentId, cancellationToken);
            if (!studentExists)
                throw new NotFoundException("Student", candidate.StudentId);

            var fee = await context.TuitionFees
                .SingleOrDefaultAsync(f => f.TuitionFeeId == candidate.TuitionFeeId, cancellationToken)
                ?? throw new NotFoundException("TuitionFee", candidate.TuitionFeeId);

            if (fee.StudentId != candidate.StudentId)
                throw new ConflictException("The tuition fee belongs to another student.");

            if (string.Equals(fee.Status, "cancelled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fee.Status, "canceled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fee.Status, "anulado", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fee.Status, "voided", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fee.Status, "void", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fee.Status, "annulled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fee.Status, "anulada", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fee.Status, "reversed", StringComparison.OrdinalIgnoreCase))
                throw new ConflictException("Payments cannot be applied to a cancelled tuition fee.");

            if (!string.Equals(fee.CurrencyCode, candidate.CurrencyCode,
                    StringComparison.OrdinalIgnoreCase))
                throw new ConflictException("Payment currency must match tuition fee currency.");

            var confirmedPaid = await context.Payments.AsNoTracking()
                .Where(payment => payment.TuitionFeeId == fee.TuitionFeeId &&
                    payment.Status != null &&
                    ConfirmedStatuses.Contains(payment.Status.ToLower()))
                .SumAsync(payment => payment.Amount, cancellationToken);

            var outstanding = fee.OriginalAmount - confirmedPaid;
            if (outstanding <= 0m || candidate.Amount > outstanding)
                throw new ConflictException("The payment exceeds the outstanding tuition fee balance.");

            var remaining = outstanding - candidate.Amount;
            fee.OutstandingBalance = remaining;
            fee.Status = remaining == 0m ? "Paid" : "Partial";
            fee.UpdatedAt = DateTime.UtcNow;

            await context.Payments.AddAsync(candidate, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return candidate;
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.SerializationFailure)
        {
            throw new ConflictException(
                "Another payment was posted concurrently. Retry using the same external identifier.");
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException(
                "A payment with this external identifier already exists. Retry the request to retrieve it.");
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.SerializationFailure)
        {
            throw new ConflictException(
                "Another payment was posted concurrently. Retry using the same external identifier.");
        }
    }
}
