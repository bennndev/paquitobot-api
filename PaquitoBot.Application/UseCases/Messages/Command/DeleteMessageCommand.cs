using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Messages.Command;

public record DeleteMessageCommand(Guid MessageId) : IRequest;

public sealed class DeleteMessageCommandValidator : AbstractValidator<DeleteMessageCommand>
{
    public DeleteMessageCommandValidator()
    {
        RuleFor(command => command.MessageId).NotEmpty();
    }
}

internal sealed class DeleteMessageCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<DeleteMessageCommand>
{
    public async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await unitOfWork.Messages.GetByIdAsync(request.MessageId, cancellationToken)
            ?? throw new NotFoundException("Message", request.MessageId);
        var conversation = await unitOfWork.Repository<Conversation>()
            .GetByIdAsync(message.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Message", request.MessageId);

        ConversationAccess.RequireOwnerOrAdministrator(conversation, currentUser);
        unitOfWork.Messages.Remove(message);
        conversation.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
