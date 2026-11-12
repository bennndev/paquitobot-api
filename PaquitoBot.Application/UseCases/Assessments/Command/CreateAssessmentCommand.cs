using MediatR;
using FluentValidation;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Assessments.Command;

public record CreateAssessmentCommand(
    Guid SectionId,
    string Title,
    string? FrappeExternalId = null,
    string? AssessmentType = null,
    decimal? MaximumScore = null,
    decimal? Weight = null,
    DateOnly? AssessmentDate = null,
    string? Status = null
) : IRequest<Guid>;

public class CreateAssessmentCommandValidator : AbstractValidator<CreateAssessmentCommand>
{
    public CreateAssessmentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(c => c.SectionId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(255);
        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
        RuleFor(c => c.AssessmentType).MaximumLength(80);
        RuleFor(c => c.MaximumScore).PrecisionScale(10, 2, ignoreTrailingZeros: true).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Weight).InclusiveBetween(0m, 100m).PrecisionScale(7, 4, ignoreTrailingZeros: true);
        RuleFor(c => c.AssessmentDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(c => c.AssessmentDate.HasValue);
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

internal sealed class CreateAssessmentCommandHandler(
    IUnitOfWork _unitOfWork
) : IRequestHandler<CreateAssessmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = new Assessment
        {
            SectionId = request.SectionId,
            Title = TextNormalizer.Required(request.Title),
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            AssessmentType = TextNormalizer.Optional(request.AssessmentType),
            MaximumScore = request.MaximumScore,
            Weight = request.Weight,
            AssessmentDate = request.AssessmentDate,
            Status = TextNormalizer.Optional(request.Status)
        };
        await _unitOfWork.Repository<Assessment>().AddAsync(assessment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return assessment.AssessmentId;
    }
}
