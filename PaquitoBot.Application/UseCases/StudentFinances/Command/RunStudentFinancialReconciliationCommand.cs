using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.StudentFinances;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.StudentFinances.Command;

// Local-only reconciliation. It does not contact Frappe or change financial balances.
public record RunStudentFinancialReconciliationCommand(Guid StudentId)
    : IRequest<StudentFinancialReconciliationDto>;

public sealed class RunStudentFinancialReconciliationCommandValidator
    : AbstractValidator<RunStudentFinancialReconciliationCommand>
{
    public RunStudentFinancialReconciliationCommandValidator()
    {
        RuleFor(command => command.StudentId).NotEmpty();
    }
}

public sealed class RunStudentFinancialReconciliationCommandHandler(
    IFinancialReadRepository financialRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RunStudentFinancialReconciliationCommand, StudentFinancialReconciliationDto>
{
    private static readonly HashSet<string> ConfirmedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "paid", "completed", "confirmed", "settled", "successful", "success"
    };

    private static readonly HashSet<string> CancelledStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "cancelled", "canceled", "void", "voided", "annulled", "anulado", "anulada", "reversed"
    };

    public async Task<StudentFinancialReconciliationDto> Handle(
        RunStudentFinancialReconciliationCommand request,
        CancellationToken cancellationToken)
    {
        if (!await financialRepository.StudentExistsAsync(request.StudentId, cancellationToken))
            throw new NotFoundException("Student", request.StudentId);

        var run = new SyncRun
        {
            EntityName = $"finance-student:{request.StudentId}",
            Status = SyncRunStatus.Running,
            StartDate = DateTime.UtcNow,
            ProcessedRecords = 0
        };

        await unitOfWork.SyncRuns.AddAsync(run, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var fees = await financialRepository.GetTuitionFeesAsync(request.StudentId, cancellationToken);
            var payments = await financialRepository.GetPaymentsAsync(request.StudentId, cancellationToken);
            var asOf = DateOnly.FromDateTime(DateTime.UtcNow);
            var issues = new List<FinancialReconciliationIssueDto>();
            var feeIds = new HashSet<Guid>(fees.Select(fee => fee.TuitionFeeId));

            foreach (var payment in payments.Where(payment => !feeIds.Contains(payment.TuitionFeeId)))
            {
                issues.Add(new FinancialReconciliationIssueDto(
                    payment.TuitionFeeId, payment.PaymentId, "PAYMENT_FEE_MISMATCH",
                    "The payment references a tuition fee outside this student account."));
            }

            foreach (var fee in fees)
            {
                var feePayments = payments.Where(payment => payment.TuitionFeeId == fee.TuitionFeeId).ToList();
                var cancelled = HasStatus(fee.Status, CancelledStatuses);
                var confirmedPaid = 0m;

                if (fee.OriginalAmount <= 0m)
                {
                    issues.Add(new FinancialReconciliationIssueDto(
                        fee.TuitionFeeId, null, "INVALID_FEE_AMOUNT",
                        "The tuition fee must have a positive original amount."));
                }

                foreach (var payment in feePayments)
                {
                    if (!HasStatus(payment.Status, ConfirmedStatuses))
                        continue; // Pending and rejected payments do not reduce the debt.

                    if (payment.Amount <= 0m)
                    {
                        issues.Add(new FinancialReconciliationIssueDto(
                            fee.TuitionFeeId, payment.PaymentId, "INVALID_PAYMENT_AMOUNT",
                            "A confirmed payment must have a positive amount."));
                        continue;
                    }

                    if (!string.Equals(payment.CurrencyCode?.Trim(), fee.CurrencyCode?.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        issues.Add(new FinancialReconciliationIssueDto(
                            fee.TuitionFeeId, payment.PaymentId, "PAYMENT_CURRENCY_MISMATCH",
                            "The payment currency differs from its tuition fee currency."));
                        continue;
                    }

                    if (!payment.PaidOn.HasValue || payment.PaidOn.Value > asOf)
                    {
                        issues.Add(new FinancialReconciliationIssueDto(
                            fee.TuitionFeeId, payment.PaymentId, "PAYMENT_DATE_INVALID",
                            "A confirmed payment has no payment date or is future-dated."));
                        continue;
                    }

                    if (cancelled)
                    {
                        issues.Add(new FinancialReconciliationIssueDto(
                            fee.TuitionFeeId, payment.PaymentId, "CANCELLED_FEE_PAYMENT",
                            "A confirmed payment belongs to a cancelled tuition fee."));
                        continue;
                    }

                    confirmedPaid += payment.Amount;
                }

                var calculatedOutstanding = cancelled ? 0m : Math.Max(0m, fee.OriginalAmount - confirmedPaid);
                if (!cancelled && confirmedPaid > fee.OriginalAmount)
                {
                    issues.Add(new FinancialReconciliationIssueDto(
                        fee.TuitionFeeId, null, "OVERPAYMENT",
                        "The confirmed payment total exceeds the original tuition fee amount."));
                }

                if (fee.OutstandingBalance.HasValue &&
                    fee.OutstandingBalance.Value != calculatedOutstanding)
                {
                    issues.Add(new FinancialReconciliationIssueDto(
                        fee.TuitionFeeId, null, "OUTSTANDING_BALANCE_MISMATCH",
                        "The reported outstanding balance differs from the calculated amount."));
                }
            }

            run.ProcessedRecords = fees.Count + payments.Count;
            run.EndDate = DateTime.UtcNow;
            run.Status = issues.Count == 0 ? SyncRunStatus.Completed : SyncRunStatus.Failed;
            run.ErrorMessage = issues.Count == 0 ? null :
                $"Local financial reconciliation detected {issues.Count} discrepancy/discrepancies.";
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new StudentFinancialReconciliationDto(
                run.SyncRunId, request.StudentId, run.Status, asOf,
                run.ProcessedRecords, fees.Count, payments.Count,
                issues.Count, issues);
        }
        catch
        {
            if (run.Status == SyncRunStatus.Running)
            {
                run.Status = SyncRunStatus.Failed;
                run.EndDate = DateTime.UtcNow;
                run.ErrorMessage = "Financial reconciliation could not complete. Check server logs.";
                try
                {
                    await unitOfWork.SaveChangesAsync(CancellationToken.None);
                }
                catch
                {
                    // Preserve the original failure if the database is also unavailable.
                }
            }

            throw;
        }
    }

    private static bool HasStatus(string? value, HashSet<string> statuses) =>
        value is not null && statuses.Contains(value.Trim());
}
