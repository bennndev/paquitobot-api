using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.KnowledgeDocuments.Command;

public record MarkKnowledgeDocumentAsIndexedUseCaseCommand(Guid DocumentId) : IRequest;

public class MarkKnowledgeDocumentAsIndexedUseCaseCommandValidator
    : AbstractValidator<MarkKnowledgeDocumentAsIndexedUseCaseCommand>
{
    public MarkKnowledgeDocumentAsIndexedUseCaseCommandValidator()
    {
        RuleFor(c => c.DocumentId).NotEmpty();
    }
}

public class MarkKnowledgeDocumentAsIndexedUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<MarkKnowledgeDocumentAsIndexedUseCaseCommand>
{
    public async Task Handle(
        MarkKnowledgeDocumentAsIndexedUseCaseCommand request,
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
        document.UpdatedAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
