using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Authenticate;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Command;

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

public class CreateConversationCommandHandler(IUnitOfWork _unitOfWork, ICurrentUser currentUser)
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

        if (!await _unitOfWork.Repository<User>().ExistsAsync(ownerId, cancellationToken))
        {
            throw new NotFoundException("User", ownerId);
        }

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
