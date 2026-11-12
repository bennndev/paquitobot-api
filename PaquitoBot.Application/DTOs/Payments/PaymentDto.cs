using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.Payments;

public record PaymentDto(
    Guid PaymentId,
    string? FrappeExternalId,
    Guid StudentId,
    Guid TuitionFeeId,
    string CurrencyCode,
    decimal Amount,
    DateOnly? PaidOn,
    string? PaymentMethod,
    string? Reference,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static PaymentDto From(Payment payment)
    {
        return new PaymentDto(
            payment.PaymentId,
            payment.FrappeExternalId,
            payment.StudentId,
            payment.TuitionFeeId,
            payment.CurrencyCode,
            payment.Amount,
            payment.PaidOn,
            payment.PaymentMethod,
            payment.Reference,
            payment.Status,
            payment.FrappeModifiedAt,
            payment.SyncedAt,
            payment.CreatedAt,
            payment.UpdatedAt);
    }
}