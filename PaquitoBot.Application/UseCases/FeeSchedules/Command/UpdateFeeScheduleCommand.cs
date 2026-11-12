using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.FeeSchedules;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeSchedules.Command;

public record UpdateFeeScheduleCommand(
    Guid FeeScheduleId,
    string Name,
    Guid? AcademicPeriodId = null,
    DateOnly? DueDate = null,
    decimal? ScheduledAmount = null,
    string? Status = null) : IRequest<FeeScheduleDto>;

public class UpdateFeeScheduleCommandValidator : AbstractValidator<UpdateFeeScheduleCommand>
{
    public UpdateFeeScheduleCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ScheduledAmount).GreaterThanOrEqualTo(0m).PrecisionScale(14, 2, false)
            .When(c => c.ScheduledAmount.HasValue);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

public class UpdateFeeScheduleCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<UpdateFeeScheduleCommand, FeeScheduleDto>
{
    public async Task<FeeScheduleDto> Handle(
        UpdateFeeScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var feeSchedule = await _unitOfWork.Repository<FeeSchedule>()
            .GetByIdAsync(request.FeeScheduleId, cancellationToken)
            ?? throw new NotFoundException("FeeSchedule", request.FeeScheduleId);

        if (request.AcademicPeriodId.HasValue
            && !await _unitOfWork.Repository<AcademicPeriod>().ExistsAsync(request.AcademicPeriodId.Value, cancellationToken))
        {
            throw new NotFoundException("AcademicPeriod", request.AcademicPeriodId.Value);
        }

        // La estructura de pago (FeeStructureId) no se cambia aquí
        feeSchedule.Name = TextNormalizer.Required(request.Name);
        feeSchedule.AcademicPeriodId = request.AcademicPeriodId;
        feeSchedule.DueDate = request.DueDate;
        feeSchedule.ScheduledAmount = request.ScheduledAmount;
        feeSchedule.Status = TextNormalizer.Optional(request.Status);
        feeSchedule.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FeeScheduleDto.From(feeSchedule);
    }
}