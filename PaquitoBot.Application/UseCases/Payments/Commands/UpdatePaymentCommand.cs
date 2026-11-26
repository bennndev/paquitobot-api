using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.Payments;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Payments.Commands;

public record UpdatePaymentCommand(
    Guid PaymentId,
    string CurrencyCode,
    decimal Amount,
    DateOnly? PaidOn = null,
    string? PaymentMethod = null,
    string? Reference = null,
    string? Status = null) : IRequest<PaymentDto>;

public class UpdatePaymentCommandValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.CurrencyCode).NotEmpty().Length(3);
        RuleFor(c => c.Amount).GreaterThan(0m).PrecisionScale(14, 2, false);
        RuleFor(c => c.PaymentMethod).MaximumLength(80);
        RuleFor(c => c.Reference).MaximumLength(140);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

internal sealed class UpdatePaymentCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<UpdatePaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(
        UpdatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Repository<Payment>()
            .GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new NotFoundException("Payment", request.PaymentId);

        payment.CurrencyCode = TextNormalizer.Required(request.CurrencyCode).ToUpperInvariant();
        payment.Amount = request.Amount;
        payment.PaidOn = request.PaidOn;
        payment.PaymentMethod = TextNormalizer.Optional(request.PaymentMethod);
        payment.Reference = TextNormalizer.Optional(request.Reference);
        payment.Status = TextNormalizer.Optional(request.Status);
        payment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PaymentDto.From(payment);
    }
}