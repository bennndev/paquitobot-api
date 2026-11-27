using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Authenticate;
using PaquitoBot.Application.Services.ChatBot;
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

public class DeleteConversationCommandHandler(IUnitOfWork _unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<DeleteConversationCommand>
{
    public async Task Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _unitOfWork.Repository<Conversation>().GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation", request.ConversationId);

        ConversationAccess.RequireOwnerOrAdministrator(conversation, currentUser);

        var messages = await _unitOfWork.Messages.GetByConversationAsync(request.ConversationId, cancellationToken);
        foreach (var message in messages)
        {
            _unitOfWork.Messages.Remove(message);
        }

        _unitOfWork.Repository<Conversation>().Remove(conversation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
