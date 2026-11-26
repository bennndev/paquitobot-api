using PaquitoBot.Application.Services;
using FluentValidation;
using MediatR;
using PaquitoBot.Application.Dtos.StudentFinances;

namespace PaquitoBot.Application.UseCases.StudentFinances.Commands;

public sealed record PreviewFrappeStudentFeesCommand(
    Guid StudentId, string SourceDocumentType = "Fees")
    : IRequest<FrappeFinancialPreviewDto>;

public sealed class PreviewFrappeStudentFeesCommandValidator
    : AbstractValidator<PreviewFrappeStudentFeesCommand>
{
    public PreviewFrappeStudentFeesCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.SourceDocumentType)
            .Must(value => string.Equals(value, "Fees", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "Sales Invoice", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SourceDocumentType must be 'Fees' or 'Sales Invoice'.");
    }
}

internal sealed class PreviewFrappeStudentFeesCommandHandler(IFrappeFeesPreviewService service)
    : IRequestHandler<PreviewFrappeStudentFeesCommand, FrappeFinancialPreviewDto>
{
    public Task<FrappeFinancialPreviewDto> Handle(PreviewFrappeStudentFeesCommand request, CancellationToken cancellationToken)
        => service.ExecuteAsync(request.StudentId, request.SourceDocumentType, cancellationToken);
}
