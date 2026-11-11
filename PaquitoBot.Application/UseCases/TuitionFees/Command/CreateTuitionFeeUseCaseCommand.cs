using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.TuitionFees;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.TuitionFees.Command;

// La moneda por defecto es PEN, igual que en la base de datos
public record CreateTuitionFeeUseCaseCommand(
    Guid StudentId,
    string Description,
    decimal OriginalAmount,
    string? FrappeExternalId = null,
    Guid? FeeScheduleId = null,
    string CurrencyCode = "PEN",
    decimal? OutstandingBalance = null,
    DateOnly? IssuedOn = null,
    DateOnly? DueDate = null,
    string? Status = null) : IRequest<TuitionFeeDto>;

public class CreateTuitionFeeUseCaseCommandValidator : AbstractValidator<CreateTuitionFeeUseCaseCommand>
{
    public CreateTuitionFeeUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.StudentId).NotEmpty();
        RuleFor(c => c.Description).NotEmpty().MaximumLength(255);
        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
        RuleFor(c => c.CurrencyCode).NotEmpty().Length(3);
        RuleFor(c => c.OriginalAmount).GreaterThanOrEqualTo(0m).PrecisionScale(14, 2, false);
        RuleFor(c => c.OutstandingBalance).GreaterThanOrEqualTo(0m).PrecisionScale(14, 2, false)
            .When(c => c.OutstandingBalance.HasValue);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

public class CreateTuitionFeeUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateTuitionFeeUseCaseCommand, TuitionFeeDto>
{
    public async Task<TuitionFeeDto> Handle(
        CreateTuitionFeeUseCaseCommand request,
        CancellationToken cancellationToken)
    {
        // El estudiante es obligatorio y debe existir
        if (!await _unitOfWork.Repository<Student>().ExistsAsync(request.StudentId, cancellationToken))
        {
            throw new NotFoundException("Student", request.StudentId);
        }

        // El cronograma es opcional, pero si lo mandan debe existir
        if (request.FeeScheduleId.HasValue
            && !await _unitOfWork.Repository<FeeSchedule>().ExistsAsync(request.FeeScheduleId.Value, cancellationToken))
        {
            throw new NotFoundException("FeeSchedule", request.FeeScheduleId.Value);
        }

        var tuitionFee = new TuitionFee
        {
            StudentId = request.StudentId,
            Description = TextNormalizer.Required(request.Description),
            OriginalAmount = request.OriginalAmount,
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            FeeScheduleId = request.FeeScheduleId,
            CurrencyCode = TextNormalizer.Required(request.CurrencyCode).ToUpperInvariant(),
            OutstandingBalance = request.OutstandingBalance,
            IssuedOn = request.IssuedOn,
            DueDate = request.DueDate,
            Status = TextNormalizer.Optional(request.Status)
        };

        await _unitOfWork.Repository<TuitionFee>().AddAsync(tuitionFee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TuitionFeeDto.From(tuitionFee);
    }
}