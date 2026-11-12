using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Command;

public record CreateConversationCommand(
    Guid UserId,
    string? Title = null) : IRequest<Guid>;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Title).MaximumLength(255);
    }
}

public class CreateConversationCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateConversationCommand, Guid>
{
    public async Task<Guid> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        if (!await _unitOfWork.Repository<User>().ExistsAsync(request.UserId, cancellationToken))
        {
            throw new NotFoundException("User", request.UserId);
        }

        var conversation = new Conversation
        {
            UserId = request.UserId,
            Title = TextNormalizer.Optional(request.Title),
            Status = "active"
        };

        await _unitOfWork.Repository<Conversation>().AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return conversation.ConversationId;
    }
}
