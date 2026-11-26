using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Commands;

public record UpdateConversationCommand(
    Guid ConversationId,
    string Status,
    string? Title = null) : IRequest;

public class UpdateConversationCommandValidator : AbstractValidator<UpdateConversationCommand>
{
    public UpdateConversationCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.ConversationId).NotEmpty();
        RuleFor(c => c.Status).NotEmpty().MaximumLength(40);
        RuleFor(c => c.Title).MaximumLength(255);
    }
}

internal sealed class UpdateConversationCommandHandler(IUnitOfWork _unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<UpdateConversationCommand>
{
    public async Task Handle(UpdateConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _unitOfWork.Repository<Conversation>().GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation", request.ConversationId);

        ConversationAccess.RequireOwnerOrAdministrator(conversation, currentUser);

        conversation.Title = TextNormalizer.Optional(request.Title);
        conversation.Status = TextNormalizer.Required(request.Status);
        conversation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
