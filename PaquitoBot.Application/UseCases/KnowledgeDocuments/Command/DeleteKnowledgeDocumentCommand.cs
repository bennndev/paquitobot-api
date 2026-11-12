using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.KnowledgeDocuments.Command;

public record DeleteKnowledgeDocumentCommand(Guid DocumentId) : IRequest;

public class DeleteKnowledgeDocumentCommandValidator : AbstractValidator<DeleteKnowledgeDocumentCommand>
{
    public DeleteKnowledgeDocumentCommandValidator()
    {
        RuleFor(c => c.DocumentId).NotEmpty();
    }
}

public class DeleteKnowledgeDocumentCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeleteKnowledgeDocumentCommand>
{
    public async Task Handle(DeleteKnowledgeDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.Repository<KnowledgeDocument>().GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeDocument", request.DocumentId);

        _unitOfWork.Repository<KnowledgeDocument>().Remove(document);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
