using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.KnowledgeDocuments.Command;

public record UpdateKnowledgeDocumentUseCaseCommand(
    Guid DocumentId,
    string Title,
    string Status,
    string? Content = null,
    string? SourceUrl = null,
    Guid? CourseId = null,
    string? DocumentType = null,
    string? Source = null) : IRequest;

public class UpdateKnowledgeDocumentUseCaseCommandValidator : AbstractValidator<UpdateKnowledgeDocumentUseCaseCommand>
{
    public UpdateKnowledgeDocumentUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.DocumentId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(255);
        RuleFor(c => c.Status).NotEmpty().MaximumLength(40);
        RuleFor(c => c.DocumentType).MaximumLength(60);
        RuleFor(c => c.Source).MaximumLength(255);
        RuleFor(c => c.CourseId).NotEqual(Guid.Empty).When(c => c.CourseId.HasValue);
        RuleFor(c => c.Content).NotEmpty()
            .When(c => string.IsNullOrWhiteSpace(c.SourceUrl))
            .WithMessage("Either Content or SourceUrl is required.");
        RuleFor(c => c.SourceUrl).Must(BeHttpUrl)
            .When(c => !string.IsNullOrWhiteSpace(c.SourceUrl))
            .WithMessage("SourceUrl must be a valid http or https URL.");
    }

    private static bool BeHttpUrl(string? value)
    {
        return Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

public class UpdateKnowledgeDocumentUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<UpdateKnowledgeDocumentUseCaseCommand>
{
    public async Task Handle(UpdateKnowledgeDocumentUseCaseCommand request, CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.Repository<KnowledgeDocument>().GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeDocument", request.DocumentId);

        if (request.CourseId is { } courseId
            && !await _unitOfWork.Repository<Course>().ExistsAsync(courseId, cancellationToken))
        {
            throw new NotFoundException("Course", courseId);
        }

        var content = TextNormalizer.Optional(request.Content);
        var contentHash = content is null ? null : ContentHasher.Sha256(content);

        if (contentHash != document.ContentHash)
        {
            document.IndexedAt = null;
        }

        document.Title = TextNormalizer.Required(request.Title);
        document.Status = TextNormalizer.Required(request.Status);
        document.CourseId = request.CourseId;
        document.DocumentType = TextNormalizer.Optional(request.DocumentType);
        document.Source = TextNormalizer.Optional(request.Source);
        document.SourceUrl = TextNormalizer.Optional(request.SourceUrl);
        document.Content = content;
        document.ContentHash = contentHash;
        document.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
