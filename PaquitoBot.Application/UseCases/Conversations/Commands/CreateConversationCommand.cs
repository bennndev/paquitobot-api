using PaquitoBot.Domain.Validations;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Commands;

public record CreateConversationCommand(
    Guid? UserId = null,
    string? Title = null) : IRequest<Guid>;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.UserId).NotEqual(Guid.Empty).When(c => c.UserId.HasValue);
        RuleFor(c => c.Title).MaximumLength(255);
    }
}

internal sealed class CreateConversationCommandHandler(IUnitOfWork _unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<CreateConversationCommand, Guid>
{
    public async Task<Guid> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("The authenticated user could not be identified.");
        if (!currentUser.IsAdministrator && request.UserId.HasValue && request.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("A conversation can only be created for the current user.");
        }

        var ownerId = currentUser.IsAdministrator ? request.UserId ?? currentUserId : currentUserId;

        await _unitOfWork.Repository<User>().EnsureExistsAsync("User", ownerId, cancellationToken);

        var conversation = new Conversation
        {
            UserId = ownerId,
            Title = TextNormalizer.Optional(request.Title),
            Status = "active"
        };

        await _unitOfWork.Repository<Conversation>().AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return conversation.ConversationId;
    }
}
