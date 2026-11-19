using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Payments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Payments.Command;

// FrappeExternalId acts as an idempotency key. Keep it stable across retries.
public record CreatePaymentCommand(
    Guid StudentId,
    Guid TuitionFeeId,
    decimal Amount,
    string? FrappeExternalId = null,
    string CurrencyCode = "PEN",
    DateOnly? PaidOn = null,
    string? PaymentMethod = null,
    string? Reference = null,
    string? Status = null) : IRequest<PaymentDto>;

public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.StudentId).NotEmpty();
        RuleFor(c => c.TuitionFeeId).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0m).PrecisionScale(14, 2, false);
        RuleFor(c => c.FrappeExternalId).NotEmpty().MaximumLength(140);
        RuleFor(c => c.CurrencyCode).NotEmpty().Length(3);
        RuleFor(c => c.PaymentMethod).MaximumLength(80);
        RuleFor(c => c.Reference).MaximumLength(140);
        RuleFor(c => c.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) ||
                string.Equals(status.Trim(), "Paid", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status.Trim(), "Completed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status.Trim(), "Confirmed", StringComparison.OrdinalIgnoreCase))
            .WithMessage("New payments must have a confirmed status: Paid, Completed or Confirmed.");
    }
}

public sealed class CreatePaymentCommandHandler(IPaymentPostingRepository repository)
    : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = new Payment
        {
            StudentId = request.StudentId,
            TuitionFeeId = request.TuitionFeeId,
            Amount = request.Amount,
            FrappeExternalId = TextNormalizer.Required(
    request.FrappeExternalId ?? throw new ArgumentNullException(nameof(request.FrappeExternalId))),
            CurrencyCode = TextNormalizer.Required(request.CurrencyCode).ToUpperInvariant(),
            PaidOn = request.PaidOn,
            PaymentMethod = TextNormalizer.Optional(request.PaymentMethod),
            Reference = TextNormalizer.Optional(request.Reference),
            Status = "Paid"
        };

        var saved = await repository.RecordAsync(payment, cancellationToken);
        return PaymentDto.From(saved);
    }
}
