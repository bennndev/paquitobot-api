using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Command;

public record UpdateConversationUseCaseCommand(
    Guid ConversationId,
    string Status,
    string? Title = null) : IRequest;

public class UpdateConversationUseCaseCommandValidator : AbstractValidator<UpdateConversationUseCaseCommand>
{
    public UpdateConversationUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.ConversationId).NotEmpty();
        RuleFor(c => c.Status).NotEmpty().MaximumLength(40);
        RuleFor(c => c.Title).MaximumLength(255);
    }
}

public class UpdateConversationUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<UpdateConversationUseCaseCommand>
{
    public async Task Handle(UpdateConversationUseCaseCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _unitOfWork.Repository<Conversation>().GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation", request.ConversationId);

        conversation.Title = TextNormalizer.Optional(request.Title);
        conversation.Status = TextNormalizer.Required(request.Status);
        conversation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
