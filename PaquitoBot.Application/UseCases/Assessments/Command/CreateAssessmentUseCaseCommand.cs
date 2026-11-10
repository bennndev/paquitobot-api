using FluentValidation;
using MediatR;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Assessments.Command;

public record CreateAssessmentUseCaseCommand(
    Guid SectionId,
    string Title,
    string? AssessmentType = null,
    decimal? MaximumScore = null,
    decimal? Weight = null,
    DateOnly? AssessmentDate = null,
    string? Status = null
) : IRequest<Guid>;

public class CreateAssessmentUseCaseCommandValidator : AbstractValidator<CreateAssessmentUseCaseCommand>
{
    public CreateAssessmentUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(c => c.SectionId).NotEmpty(); 
        RuleFor(c => c.Title).NotEmpty().MaximumLength(255);
        RuleFor(c => c.AssessmentType).MaximumLength(80);
        RuleFor(c => c.MaximumScore).PrecisionScale(10, 2, ignoreTrailingZeros: true).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Weight).InclusiveBetween(0m, 100m).PrecisionScale(7, 4, ignoreTrailingZeros: true);;
        RuleFor(c => c.AssessmentDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(c => c.AssessmentDate.HasValue); //Ejecutar unicamente cuando date tenga valor 
        RuleFor(c => c.Status).MaximumLength(40);
    }
}

// internal sealed class CreateAssessmentUseCaseCommandHandler(
//     IGenericRepository<Assessment> _assessments,
//     IUnitOfWork _unitOfWork
// ) : IRequestHandler<CreateAssessmentUseCaseCommand, Guid>
// {
// }
//
