using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.KnowledgeDocuments.Commands;

public record MarkKnowledgeDocumentAsIndexedCommand(Guid DocumentId) : IRequest;

public class MarkKnowledgeDocumentAsIndexedCommandValidator
    : AbstractValidator<MarkKnowledgeDocumentAsIndexedCommand>
{
    public MarkKnowledgeDocumentAsIndexedCommandValidator()
    {
        RuleFor(c => c.DocumentId).NotEmpty();
    }
}

internal sealed class MarkKnowledgeDocumentAsIndexedCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<MarkKnowledgeDocumentAsIndexedCommand>
{
    public async Task Handle(
        MarkKnowledgeDocumentAsIndexedCommand request,
        CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.Repository<KnowledgeDocument>().GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeDocument", request.DocumentId);

        if (string.IsNullOrWhiteSpace(document.Content))
        {
            throw new ConflictException($"Knowledge document '{request.DocumentId}' has no content to index.");
        }

        var now = DateTime.UtcNow;

        document.IndexedAt = now;
        document.Status = KnowledgeDocumentStatus.Indexed;
        document.UpdatedAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
