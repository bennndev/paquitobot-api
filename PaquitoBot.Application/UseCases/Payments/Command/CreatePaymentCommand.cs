using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Payments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Payments.Command;

// La moneda por defecto es PEN, igual que en la base de datos
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

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.StudentId).NotEmpty();
        RuleFor(c => c.TuitionFeeId).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0m).PrecisionScale(14, 2, false);
        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
        RuleFor(c => c.CurrencyCode).NotEmpty().Length(3);
        RuleFor(c => c.PaymentMethod).MaximumLength(80);
        RuleFor(c => c.Reference).MaximumLength(140);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

public class CreatePaymentCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(
        CreatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _unitOfWork.Repository<Student>().ExistsAsync(request.StudentId, cancellationToken))
        {
            throw new NotFoundException("Student", request.StudentId);
        }

        var tuitionFee = await _unitOfWork.Repository<TuitionFee>()
            .GetByIdAsync(request.TuitionFeeId, cancellationToken)
            ?? throw new NotFoundException("TuitionFee", request.TuitionFeeId);

        // El pago debe ser del mismo estudiante dueño de la pensión
        if (tuitionFee.StudentId != request.StudentId)
        {
            throw new ConflictException(
                $"Tuition fee '{request.TuitionFeeId}' does not belong to student '{request.StudentId}'.");
        }

        var payment = new Payment
        {
            StudentId = request.StudentId,
            TuitionFeeId = request.TuitionFeeId,
            Amount = request.Amount,
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            CurrencyCode = TextNormalizer.Required(request.CurrencyCode).ToUpperInvariant(),
            PaidOn = request.PaidOn,
            PaymentMethod = TextNormalizer.Optional(request.PaymentMethod),
            Reference = TextNormalizer.Optional(request.Reference),
            Status = TextNormalizer.Optional(request.Status)
        };

        await _unitOfWork.Repository<Payment>().AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PaymentDto.From(payment);
    }
}