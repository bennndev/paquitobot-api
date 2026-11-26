using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.StudentFinances;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.StudentFinances.Queries;

public record GetStudentFinancialStatementQuery(
    Guid StudentId,
    DateOnly? AsOf = null) : IRequest<StudentFinancialStatementDto>;

public sealed class GetStudentFinancialStatementQueryValidator
    : AbstractValidator<GetStudentFinancialStatementQuery>
{
    public GetStudentFinancialStatementQueryValidator()
    {
        RuleFor(query => query.StudentId).NotEmpty();
    }
}

internal sealed class GetStudentFinancialStatementQueryHandler(IFinancialReadRepository repository, ICurrentUser currentUser)
    : IRequestHandler<GetStudentFinancialStatementQuery, StudentFinancialStatementDto>
{
    private static readonly HashSet<string> ConfirmedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "paid", "completed", "confirmed", "settled", "successful", "success"
    };

    private static readonly HashSet<string> CancelledStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "cancelled", "canceled", "void", "voided", "annulled", "anulado", "anulada", "reversed"
    };

    public async Task<StudentFinancialStatementDto> Handle(
        GetStudentFinancialStatementQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException("The PaquitoBot user is not authenticated.");
        if ((!currentUser.IsAdministrator && !await repository.CanUserAccessStudentAsync(
                userId, request.StudentId, cancellationToken)) ||
            !await repository.StudentExistsAsync(request.StudentId, cancellationToken))
        {
            throw new NotFoundException("Student", request.StudentId);
        }

        var asOf = request.AsOf ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var isCurrentStatement = asOf == DateOnly.FromDateTime(DateTime.UtcNow);
        var fees = await repository.GetTuitionFeesAsync(request.StudentId, cancellationToken);
        var payments = await repository.GetPaymentsAsync(request.StudentId, cancellationToken);
        var appliedIds = new HashSet<Guid>();
        var obligations = new List<TuitionFeeBalanceDto>(fees.Count);

        foreach (var fee in fees)
        {
            var cancelled = HasStatus(fee.Status, CancelledStatuses);
            var currency = NormalizeCurrency(fee.CurrencyCode);
            var amount = Math.Max(0m, fee.OriginalAmount);

            var matching = payments.Where(payment =>
                !cancelled &&
                payment.TuitionFeeId == fee.TuitionFeeId &&
                payment.Amount > 0m &&
                payment.PaidOn.HasValue && payment.PaidOn.Value <= asOf &&
                HasStatus(payment.Status, ConfirmedStatuses) &&
                NormalizeCurrency(payment.CurrencyCode) == currency).ToList();

            foreach (var payment in matching)
            {
                appliedIds.Add(payment.PaymentId);
            }

            var paid = matching.Sum(payment => payment.Amount);
            var outstanding = cancelled ? 0m : Math.Max(0m, amount - paid);
            var excess = cancelled ? 0m : Math.Max(0m, paid - amount);

            var calculatedStatus = cancelled ? "Cancelled" :
                outstanding == 0m ? "Paid" :
                fee.DueDate < asOf ? "Overdue" :
                paid > 0m ? "Partial" : "Pending";

            var reconciliationRequired = isCurrentStatement && !cancelled &&
                fee.OutstandingBalance.HasValue && fee.OutstandingBalance.Value != outstanding;

            obligations.Add(new TuitionFeeBalanceDto(
                fee.TuitionFeeId, fee.FeeScheduleId, fee.Description, currency, amount,
                paid, outstanding, excess, fee.OutstandingBalance,
                reconciliationRequired, fee.DueDate, calculatedStatus, cancelled));
        }

        var summaries = obligations.Where(item => !item.IsCancelled)
            .GroupBy(item => item.CurrencyCode)
            .OrderBy(group => group.Key)
            .Select(group => new CurrencyFinancialSummaryDto(
                group.Key,
                group.Sum(item => item.OriginalAmount),
                group.Sum(item => item.ConfirmedPaidAmount),
                group.Sum(item => item.OutstandingAmount),
                group.Where(item => item.DueDate < asOf)
                    .Sum(item => item.OutstandingAmount),
                group.Sum(item => item.ExcessPaymentAmount),
                group.Where(item => item.OutstandingAmount > 0m && item.DueDate >= asOf)
                    .Min(item => item.DueDate)))
            .ToList();

        return new StudentFinancialStatementDto(
            request.StudentId, asOf, summaries, obligations,
            payments.Count - appliedIds.Count);
    }

    private static bool HasStatus(string? value, HashSet<string> statuses) =>
        value is not null && statuses.Contains(value.Trim());

    private static string NormalizeCurrency(string? currency) =>
        string.IsNullOrWhiteSpace(currency) ? "UNSPECIFIED" : currency.Trim().ToUpperInvariant();
}
