using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.TuitionFees;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.TuitionFees.Commands;

public record UpdateTuitionFeeCommand(
    Guid TuitionFeeId,
    string Description,
    string CurrencyCode,
    decimal OriginalAmount,
    Guid? FeeScheduleId = null,
    decimal? OutstandingBalance = null,
    DateOnly? IssuedOn = null,
    DateOnly? DueDate = null,
    string? Status = null) : IRequest<TuitionFeeDto>;

public class UpdateTuitionFeeCommandValidator : AbstractValidator<UpdateTuitionFeeCommand>
{
    public UpdateTuitionFeeCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Description).NotEmpty().MaximumLength(255);
        RuleFor(c => c.CurrencyCode).NotEmpty().Length(3);
        RuleFor(c => c.OriginalAmount).GreaterThanOrEqualTo(0m).PrecisionScale(14, 2, false);
        RuleFor(c => c.OutstandingBalance).GreaterThanOrEqualTo(0m).PrecisionScale(14, 2, false)
            .When(c => c.OutstandingBalance.HasValue);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

internal sealed class UpdateTuitionFeeCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<UpdateTuitionFeeCommand, TuitionFeeDto>
{
    public async Task<TuitionFeeDto> Handle(
        UpdateTuitionFeeCommand request,
        CancellationToken cancellationToken)
    {
        var tuitionFee = await _unitOfWork.Repository<TuitionFee>()
            .GetByIdAsync(request.TuitionFeeId, cancellationToken)
            ?? throw new NotFoundException("TuitionFee", request.TuitionFeeId);

        if (request.FeeScheduleId.HasValue
            && !await _unitOfWork.Repository<FeeSchedule>().ExistsAsync(request.FeeScheduleId.Value, cancellationToken))
        {
            throw new NotFoundException("FeeSchedule", request.FeeScheduleId.Value);
        }

        tuitionFee.Description = TextNormalizer.Required(request.Description);
        tuitionFee.CurrencyCode = TextNormalizer.Required(request.CurrencyCode).ToUpperInvariant();
        tuitionFee.OriginalAmount = request.OriginalAmount;
        tuitionFee.FeeScheduleId = request.FeeScheduleId;
        tuitionFee.OutstandingBalance = request.OutstandingBalance;
        tuitionFee.IssuedOn = request.IssuedOn;
        tuitionFee.DueDate = request.DueDate;
        tuitionFee.Status = TextNormalizer.Optional(request.Status);
        tuitionFee.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TuitionFeeDto.From(tuitionFee);
    }
}