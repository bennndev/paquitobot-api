
using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.AcademicPeriods;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AcademicPeriods.Command;

public record UpdateAcademicPeriodUseCaseCommand(
    Guid AcademicPeriodId,
    string Name,
    string? Code = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    string? Status = null) : IRequest<AcademicPeriodDto>;

public class UpdateAcademicPeriodUseCaseCommandValidator
    : AbstractValidator<UpdateAcademicPeriodUseCaseCommand>
{
    public UpdateAcademicPeriodUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.AcademicPeriodId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .MaximumLength(160);

        RuleFor(x => x.Code)
            .MaximumLength(80);

        RuleFor(x => x.Status)
            .MaximumLength(40);

        RuleFor(x => x.EndDate)
            .Must((request, endDate) =>
                !request.StartDate.HasValue ||
                !endDate.HasValue ||
                endDate.Value >= request.StartDate.Value)
            .WithMessage("EndDate must be on or after StartDate.");
    }
}

public class UpdateAcademicPeriodUseCaseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAcademicPeriodUseCaseCommand, AcademicPeriodDto>
{
    public async Task<AcademicPeriodDto> Handle(
        UpdateAcademicPeriodUseCaseCommand request,
        CancellationToken cancellationToken)
    {
        var period = await unitOfWork.Repository<AcademicPeriod>()
            .GetByIdAsync(
                request.AcademicPeriodId,
                cancellationToken)
            ?? throw new NotFoundException(
                "AcademicPeriod",
                request.AcademicPeriodId);

        period.Name = TextNormalizer.Required(request.Name);
        period.Code = TextNormalizer.Optional(request.Code);
        period.StartDate = request.StartDate;
        period.EndDate = request.EndDate;
        period.Status = TextNormalizer.Optional(request.Status);
        period.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AcademicPeriodDto.From(period);
    }
}
