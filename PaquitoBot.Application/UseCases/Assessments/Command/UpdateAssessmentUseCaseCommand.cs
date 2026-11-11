using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Assessments.Command;

public record UpdateAssessmentUseCaseCommand(
    Guid AssessmentId,
    Guid SectionId,
    string Title,
    string? FrappeExternalId = null,
    string? AssessmentType = null,
    decimal? MaximumScore = null,
    decimal? Weight = null,
    DateOnly? AssessmentDate = null,
    string? Status = null
) : IRequest<Guid>;

public class UpdateAssessmentUseCaseCommandValidator : AbstractValidator<UpdateAssessmentUseCaseCommand>
{
    public UpdateAssessmentUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(c => c.AssessmentId).NotEmpty();
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

internal sealed class UpdateAssessmentUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<UpdateAssessmentUseCaseCommand, Guid>
{
    public async Task<Guid> Handle(UpdateAssessmentUseCaseCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _unitOfWork.Repository<Assessment>().GetByIdAsync(request.AssessmentId, cancellationToken)
                         ?? throw new NotFoundException("Assessment", request.AssessmentId);

        assessment.SectionId = request.SectionId;
        assessment.Title = TextNormalizer.Required(request.Title);
        assessment.FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId);
        assessment.AssessmentType = TextNormalizer.Optional(request.AssessmentType);
        assessment.MaximumScore = request.MaximumScore;
        assessment.Weight = request.Weight;
        assessment.AssessmentDate = request.AssessmentDate;
        assessment.Status = TextNormalizer.Optional(request.Status);
        assessment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return assessment.AssessmentId;
    }
}
