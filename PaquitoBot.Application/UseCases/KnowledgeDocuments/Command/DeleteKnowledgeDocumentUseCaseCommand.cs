using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.KnowledgeDocuments.Command;

public record DeleteKnowledgeDocumentUseCaseCommand(Guid DocumentId) : IRequest;

public class DeleteKnowledgeDocumentUseCaseCommandValidator : AbstractValidator<DeleteKnowledgeDocumentUseCaseCommand>
{
    public DeleteKnowledgeDocumentUseCaseCommandValidator()
    {
        RuleFor(c => c.DocumentId).NotEmpty();
    }
}

public class DeleteKnowledgeDocumentUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeleteKnowledgeDocumentUseCaseCommand>
{
    public async Task Handle(DeleteKnowledgeDocumentUseCaseCommand request, CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.Repository<KnowledgeDocument>().GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeDocument", request.DocumentId);

        _unitOfWork.Repository<KnowledgeDocument>().Remove(document);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
