using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Command;

public record DeleteConversationCommand(Guid ConversationId) : IRequest;

public class DeleteConversationCommandValidator : AbstractValidator<DeleteConversationCommand>
{
    public DeleteConversationCommandValidator()
    {
        RuleFor(c => c.ConversationId).NotEmpty();
    }
}

public class DeleteConversationCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeleteConversationCommand>
{
    public async Task Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _unitOfWork.Repository<Conversation>().GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation", request.ConversationId);

        _unitOfWork.Repository<Conversation>().Remove(conversation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
