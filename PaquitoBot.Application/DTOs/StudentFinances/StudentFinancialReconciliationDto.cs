namespace PaquitoBot.Application.DTOs.StudentFinances;

public sealed record FinancialReconciliationIssueDto(
    Guid? TuitionFeeId,
    Guid? PaymentId,
    string Code,
    string Message);

public sealed record StudentFinancialReconciliationDto(
    Guid SyncRunId,
    Guid StudentId,
    string Status,
    DateOnly AsOf,
    int ProcessedRecords,
    int TuitionFeesChecked,
    int PaymentsChecked,
    int Discrepancies,
    IReadOnlyList<FinancialReconciliationIssueDto> Issues);
