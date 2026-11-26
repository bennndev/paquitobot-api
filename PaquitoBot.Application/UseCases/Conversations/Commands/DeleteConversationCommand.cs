using PaquitoBot.Application.Helpers;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Commands;

public record DeleteConversationCommand(Guid ConversationId) : IRequest;

public class DeleteConversationCommandValidator : AbstractValidator<DeleteConversationCommand>
{
    public DeleteConversationCommandValidator()
    {
        RuleFor(c => c.ConversationId).NotEmpty();
    }
}

internal sealed class DeleteConversationCommandHandler(IUnitOfWork _unitOfWork, ICurrentUser currentUser)
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
