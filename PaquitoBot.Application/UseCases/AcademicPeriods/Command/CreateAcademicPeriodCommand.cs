
using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.AcademicPeriods;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AcademicPeriods.Command;

public record CreateAcademicPeriodCommand(
    string Name,
    string? FrappeExternalId = null,
    string? Code = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    string? Status = null) : IRequest<AcademicPeriodDto>;

public class CreateAcademicPeriodCommandValidator
    : AbstractValidator<CreateAcademicPeriodCommand>
{
    public CreateAcademicPeriodCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .MaximumLength(160);

        RuleFor(x => x.FrappeExternalId)
            .MaximumLength(140);

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

public class CreateAcademicPeriodCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAcademicPeriodCommand, AcademicPeriodDto>
{
    public async Task<AcademicPeriodDto> Handle(
        CreateAcademicPeriodCommand request,
        CancellationToken cancellationToken)
    {
        var period = new AcademicPeriod
        {
            Name = TextNormalizer.Required(request.Name),
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            Code = TextNormalizer.Optional(request.Code),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = TextNormalizer.Optional(request.Status)
        };

        await unitOfWork.Repository<AcademicPeriod>()
            .AddAsync(period, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AcademicPeriodDto.From(period);
    }
}
