using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.StudentFinances;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.Integrations.Frappe.Finance;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.StudentFinances.Command;

// Read-only inspection. Never changes TuitionFee, Payment, FeeSchedule, or FeeStructure.
// A real import must separately reconcile Payment Entry references and Sales Invoices.
public sealed record PreviewFrappeStudentFeesCommand(
    Guid StudentId, string SourceDocumentType = "Fees")
    : IRequest<FrappeFinancialPreviewDto>;

public sealed class PreviewFrappeStudentFeesCommandValidator
    : AbstractValidator<PreviewFrappeStudentFeesCommand>
{
    public PreviewFrappeStudentFeesCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.SourceDocumentType)
            .Must(value => string.Equals(value, "Fees", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "Sales Invoice", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SourceDocumentType must be 'Fees' or 'Sales Invoice'.");
    }
}

public sealed class PreviewFrappeStudentFeesCommandHandler(
    IFrappeClient frappe,
    IFrappeSyncRunService syncRun,
    IUnitOfWork unitOfWork,
    IFinancialReadRepository financialRepository)
    : IRequestHandler<PreviewFrappeStudentFeesCommand, FrappeFinancialPreviewDto>
{
    private const int PageSize = 100;
    private const int MaxSourceRecords = 5000;

    private static readonly string[] FeesFields =
    [
        "name", "student", "fee_structure", "fee_schedule", "currency",
        "grand_total", "outstanding_amount", "posting_date", "due_date",
        "modified", "docstatus"
    ];

    // Education v15 can create Sales Invoice instead of Fees. On Education
    // sites these student/fee_schedule links are custom fields; verify the
    // running instance exposes them before using this source.
    private static readonly string[] SalesInvoiceFields =
    [
        "name", "student", "fee_schedule", "currency", "grand_total",
        "outstanding_amount", "posting_date", "due_date", "modified", "docstatus"
    ];

    public async Task<FrappeFinancialPreviewDto> Handle(
        PreviewFrappeStudentFeesCommand request, CancellationToken cancellationToken)
    {
        var student = await unitOfWork.Repository<Student>()
            .GetByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException("Student", request.StudentId);

        if (string.IsNullOrWhiteSpace(student.FrappeExternalId))
            throw new ConflictException("Student has no FrappeExternalId. Financial preview is unavailable.");

        var externalId = student.FrappeExternalId.Trim();
        var sourceDocType = string.Equals(request.SourceDocumentType, "Sales Invoice",
            StringComparison.OrdinalIgnoreCase) ? "Sales Invoice" : "Fees";
        var isSalesInvoice = sourceDocType == "Sales Invoice";
        var runId = await syncRun.StartAsync(
            $"finance-frappe-preview:{sourceDocType.Replace(' ', '-')}:{request.StudentId}",
            cancellationToken);
        var processed = 0;

        try
        {
            var source = new List<FrappeFeesRow>();
            var offset = 0;

            // The PR #44 client advances by the actual page length.
            // Do not infer end-of-data from a short, non-empty page.
            while (true)
            {
                var page = await frappe.GetPageAsync<FrappeFeesRow>(
                    new FrappeQuery(
                        sourceDocType,
                        isSalesInvoice ? SalesInvoiceFields : FeesFields,
                        [new FrappeFilter("student", "=", externalId)],
                        Offset: offset,
                        PageSize: PageSize),
                    cancellationToken);

                if (page.Items.Count == 0)
                    break;

                if (source.Count > MaxSourceRecords - page.Items.Count)
                    throw new ConflictException("Preview exceeds the 5000-row safety limit.");

                source.AddRange(page.Items);
                processed = source.Count;

                if (!page.NextOffset.HasValue)
                    break;

                if (page.NextOffset.Value <= offset)
                    throw new ConflictException("Frappe returned an invalid pagination cursor.");

                offset = page.NextOffset.Value;
            }

            var duplicates = source
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name!.Trim(), StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.Ordinal);

            var local = await financialRepository.GetTuitionFeesAsync(
                request.StudentId, cancellationToken);
            var localByExternalId = local
                .Where(fee => !string.IsNullOrWhiteSpace(fee.FrappeExternalId))
                .GroupBy(fee => fee.FrappeExternalId!.Trim(), StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            var items = source.Select(row =>
            {
                TuitionFee? match = null;
                if (!string.IsNullOrWhiteSpace(row.Name))
                    localByExternalId.TryGetValue(row.Name.Trim(), out match);

                var evaluated = FrappeFeesPreviewRules.Evaluate(
                    row, externalId, match, requireFeeStructure: !isSalesInvoice);
                if (!duplicates.Contains(evaluated.FrappeExternalId))
                    return evaluated;

                return evaluated with
                {
                    Issues = evaluated.Issues.Append("DUPLICATE_SOURCE_EXTERNAL_ID").ToList()
                };
            }).ToList();

            var warnings = new List<string>
            {
                "READ_ONLY: nothing was imported or modified.",
                "Fee Structure and Fee Schedule links are not yet validated against remote parent documents.",
                "Payment Entry allocations are not imported; do not infer student payments from its total."
            };

            if (isSalesInvoice)
            {
                warnings.Add(
                    "Sales Invoice requires custom Education student/fee_schedule fields. " +
                    "Its Fee Structure link is not part of the standard Sales Invoice contract.");
            }

            if (source.Count == 0)
            {
                warnings.Add(
                    isSalesInvoice
                        ? "No student Sales Invoice found. Verify Education custom fields and data."
                        : "No Fees found. Frappe Education v15 may use Sales Invoice; " +
                          "try source=Sales%20Invoice. Empty data is not proof of no debt.");
            }

            await syncRun.CompleteAsync(runId, processed, cancellationToken);

            return new FrappeFinancialPreviewDto(
                runId,
                request.StudentId,
                sourceDocType,
                processed,
                items.Count(item => item.DocumentState == "Submitted"),
                items.Count(item => item.ExistsLocally),
                items.Sum(item => item.Issues.Count),
                SafeToApply: false,
                warnings,
                items);
        }
        catch (Exception error)
        {
            // The failed audit uses an independent, short token even after request cancellation.
            using var auditTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                await syncRun.FailAsync(runId, processed, error, auditTimeout.Token);
            }
            catch
            {
                // Do not mask the originating failure with an audit problem.
            }

            throw;
        }
    }
}
