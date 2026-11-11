using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Command;

public record CreateAssessmentResultUseCaseCommand(
    Guid AssessmentId,
    Guid StudentId,
    string? FrappeExternalId = null,
    decimal? Grade = null,
    string? Notes = null
) : IRequest<Guid>;

public class CreateAssessmentResultUseCaseCommandValidator : AbstractValidator<CreateAssessmentResultUseCaseCommand>
{
    public CreateAssessmentResultUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(c => c.AssessmentId).NotEmpty();
        RuleFor(c => c.StudentId).NotEmpty();
        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
        RuleFor(c => c.Grade).PrecisionScale(10, 2, ignoreTrailingZeros: true).GreaterThanOrEqualTo(0);
    }
}

internal sealed class CreateAssessmentResultUseCaseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAssessmentResultUseCaseCommand, Guid>
{
    public async Task<Guid> Handle(CreateAssessmentResultUseCaseCommand request, CancellationToken cancellationToken)
    {
        var result = new AssessmentResult
        {
            AssessmentId = request.AssessmentId,
            StudentId = request.StudentId,
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            Grade = request.Grade,
            Notes = TextNormalizer.Optional(request.Notes)
        };

        await unitOfWork.Repository<AssessmentResult>().AddAsync(result, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.ResultId;
    }
}
