using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AssessmentsResults.Command;

public record UpdateAssessmentResultUseCaseCommand(
    Guid ResultId,
    Guid AssessmentId,
    Guid StudentId,
    string? FrappeExternalId = null,
    decimal? Grade = null,
    string? Notes = null
) : IRequest<Guid>;

public class UpdateAssessmentResultUseCaseCommandValidator : AbstractValidator<UpdateAssessmentResultUseCaseCommand>
{
    public UpdateAssessmentResultUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(c => c.ResultId).NotEmpty();
        RuleFor(c => c.AssessmentId).NotEmpty();
        RuleFor(c => c.StudentId).NotEmpty();
        RuleFor(c => c.FrappeExternalId).MaximumLength(140);
        RuleFor(c => c.Grade).PrecisionScale(10, 2, ignoreTrailingZeros: true).GreaterThanOrEqualTo(0);
    }
}

internal sealed class UpdateAssessmentResultUseCaseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAssessmentResultUseCaseCommand, Guid>
{
    public async Task<Guid> Handle(UpdateAssessmentResultUseCaseCommand request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Repository<AssessmentResult>().GetByIdAsync(request.ResultId, cancellationToken)
            ?? throw new NotFoundException("AssessmentResult", request.ResultId);

        result.AssessmentId = request.AssessmentId;
        result.StudentId = request.StudentId;
        result.FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId);
        result.Grade = request.Grade;
        result.Notes = TextNormalizer.Optional(request.Notes);
        result.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.ResultId;
    }
}
