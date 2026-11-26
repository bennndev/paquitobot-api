using PaquitoBot.Application.Services;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.KnowledgeDocuments.Commands;

public record CreateKnowledgeDocumentCommand(
    string Title,
    string? Content = null,
    string? SourceUrl = null,
    Guid? CourseId = null,
    string? DocumentType = null,
    string? Source = null) : IRequest<Guid>;

public class CreateKnowledgeDocumentCommandValidator : AbstractValidator<CreateKnowledgeDocumentCommand>
{
    public CreateKnowledgeDocumentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Title).NotEmpty().MaximumLength(255);
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

internal sealed class CreateKnowledgeDocumentCommandHandler(IUnitOfWork _unitOfWork, IContentHasher contentHasher)
    : IRequestHandler<CreateKnowledgeDocumentCommand, Guid>
{
    public async Task<Guid> Handle(CreateKnowledgeDocumentCommand request, CancellationToken cancellationToken)
    {
        if (request.CourseId is { } courseId
            && !await _unitOfWork.Repository<Course>().ExistsAsync(courseId, cancellationToken))
        {
            throw new NotFoundException("Course", courseId);
        }

        var content = TextNormalizer.Optional(request.Content);

        var document = new KnowledgeDocument
        {
            Title = TextNormalizer.Required(request.Title),
            CourseId = request.CourseId,
            DocumentType = TextNormalizer.Optional(request.DocumentType),
            Source = TextNormalizer.Optional(request.Source),
            SourceUrl = TextNormalizer.Optional(request.SourceUrl),
            Content = content,
            ContentHash = content is null ? null : contentHasher.Sha256(content),
            Status = KnowledgeDocumentStatus.Pending
        };

        await _unitOfWork.Repository<KnowledgeDocument>().AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return document.DocumentId;
    }
}
