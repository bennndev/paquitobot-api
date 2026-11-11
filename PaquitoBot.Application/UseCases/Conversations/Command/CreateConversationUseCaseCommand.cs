using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Command;

public record CreateConversationUseCaseCommand(
    Guid UserId,
    string? Title = null) : IRequest<Guid>;

public class CreateConversationUseCaseCommandValidator : AbstractValidator<CreateConversationUseCaseCommand>
{
    public CreateConversationUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Title).MaximumLength(255);
    }
}

public class CreateConversationUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateConversationUseCaseCommand, Guid>
{
    public async Task<Guid> Handle(CreateConversationUseCaseCommand request, CancellationToken cancellationToken)
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
