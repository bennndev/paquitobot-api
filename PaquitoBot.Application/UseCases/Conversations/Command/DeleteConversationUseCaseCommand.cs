using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Command;

public record DeleteConversationUseCaseCommand(Guid ConversationId) : IRequest;

public class DeleteConversationUseCaseCommandValidator : AbstractValidator<DeleteConversationUseCaseCommand>
{
    public DeleteConversationUseCaseCommandValidator()
    {
        RuleFor(c => c.ConversationId).NotEmpty();
    }
}

public class DeleteConversationUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeleteConversationUseCaseCommand>
{
    public async Task Handle(DeleteConversationUseCaseCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _unitOfWork.Repository<Conversation>().GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation", request.ConversationId);

        _unitOfWork.Repository<Conversation>().Remove(conversation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
