using PaquitoBot.Domain.Validations;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.TuitionFees;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.TuitionFees.Commands;

public record CreateTuitionFeeCommand(
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

public class CreateTuitionFeeCommandValidator : AbstractValidator<CreateTuitionFeeCommand>
{
    public CreateTuitionFeeCommandValidator()
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

internal sealed class CreateTuitionFeeCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateTuitionFeeCommand, TuitionFeeDto>
{
    public async Task<TuitionFeeDto> Handle(
        CreateTuitionFeeCommand request,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.Repository<Student>().EnsureExistsAsync("Student", request.StudentId, cancellationToken);

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