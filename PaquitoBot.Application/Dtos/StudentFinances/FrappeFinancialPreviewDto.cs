namespace PaquitoBot.Application.Dtos.StudentFinances;

public sealed record FrappeFinancialPreviewItemDto(
    string FrappeExternalId,
    string? FeeStructureExternalId,
    string? FeeScheduleExternalId,
    string? CurrencyCode,
    decimal? OriginalAmount,
    decimal? ExternalOutstandingAmount,
    DateOnly? DueDate,
    string DocumentState,
    bool ExistsLocally,
    IReadOnlyList<string> Issues);

public sealed record FrappeFinancialPreviewDto(
    Guid SyncRunId,
    Guid StudentId,
    string SourceDocumentType,
    int SourceRecords,
    int SubmittedDocuments,
    int LocalMatches,
    int IssueCount,
    bool SafeToApply,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<FrappeFinancialPreviewItemDto> Items);
