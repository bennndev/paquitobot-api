using PaquitoBot.Domain.Validations;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.FeeSchedules;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeSchedules.Commands;

public record CreateFeeScheduleCommand(
    Guid FeeStructureId,
    string Name,
    string? FrappeExternalId = null,
    Guid? AcademicPeriodId = null,
    DateOnly? DueDate = null,
    decimal? ScheduledAmount = null,
    string? Status = null) : IRequest<FeeScheduleDto>;

public class CreateFeeScheduleCommandValidator : AbstractValidator<CreateFeeScheduleCommand>
{
    public CreateFeeScheduleCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.FeeStructureId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
        RuleFor(c => c.ScheduledAmount).GreaterThanOrEqualTo(0m).PrecisionScale(14, 2, false)
            .When(c => c.ScheduledAmount.HasValue);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

internal sealed class CreateFeeScheduleCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateFeeScheduleCommand, FeeScheduleDto>
{
    public async Task<FeeScheduleDto> Handle(
        CreateFeeScheduleCommand request,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.Repository<FeeStructure>().EnsureExistsAsync("FeeStructure", request.FeeStructureId, cancellationToken);

        if (request.AcademicPeriodId.HasValue
            && !await _unitOfWork.Repository<AcademicPeriod>().ExistsAsync(request.AcademicPeriodId.Value, cancellationToken))
        {
            throw new NotFoundException("AcademicPeriod", request.AcademicPeriodId.Value);
        }

        var feeSchedule = new FeeSchedule
        {
            FeeStructureId = request.FeeStructureId,
            Name = TextNormalizer.Required(request.Name),
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            AcademicPeriodId = request.AcademicPeriodId,
            DueDate = request.DueDate,
            ScheduledAmount = request.ScheduledAmount,
            Status = TextNormalizer.Optional(request.Status)
        };

        await _unitOfWork.Repository<FeeSchedule>().AddAsync(feeSchedule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FeeScheduleDto.From(feeSchedule);
    }
}