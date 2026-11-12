using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.TuitionFees;

public record TuitionFeeDto(
    Guid TuitionFeeId,
    string? FrappeExternalId,
    Guid StudentId,
    Guid? FeeScheduleId,
    string Description,
    string CurrencyCode,
    decimal OriginalAmount,
    decimal? OutstandingBalance,
    DateOnly? IssuedOn,
    DateOnly? DueDate,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static TuitionFeeDto From(TuitionFee tuitionFee)
    {
        return new TuitionFeeDto(
            tuitionFee.TuitionFeeId,
            tuitionFee.FrappeExternalId,
            tuitionFee.StudentId,
            tuitionFee.FeeScheduleId,
            tuitionFee.Description,
            tuitionFee.CurrencyCode,
            tuitionFee.OriginalAmount,
            tuitionFee.OutstandingBalance,
            tuitionFee.IssuedOn,
            tuitionFee.DueDate,
            tuitionFee.Status,
            tuitionFee.FrappeModifiedAt,
            tuitionFee.SyncedAt,
            tuitionFee.CreatedAt,
            tuitionFee.UpdatedAt);
    }
}