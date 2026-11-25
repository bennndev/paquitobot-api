namespace PaquitoBot.Application.DTOs.StudentFinances;

public sealed record TuitionFeeBalanceDto(
    Guid TuitionFeeId,
    Guid? FeeScheduleId,
    string Description,
    string CurrencyCode,
    decimal OriginalAmount,
    decimal ConfirmedPaidAmount,
    decimal OutstandingAmount,
    decimal ExcessPaymentAmount,
    decimal? ReportedOutstandingBalance,
    bool ReconciliationRequired,
    DateOnly? DueDate,
    string CalculatedStatus,
    bool IsCancelled);

public sealed record CurrencyFinancialSummaryDto(
    string CurrencyCode,
    decimal TotalScheduled,
    decimal TotalPaid,
    decimal TotalOutstanding,
    decimal TotalOverdue,
    decimal ExcessPaymentAmount,
    DateOnly? NextDueDate);

public sealed record StudentFinancialStatementDto(
    Guid StudentId,
    DateOnly AsOf,
    IReadOnlyList<CurrencyFinancialSummaryDto> Currencies,
    IReadOnlyList<TuitionFeeBalanceDto> Obligations,
    int PaymentsNotCounted);
