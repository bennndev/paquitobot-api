using PaquitoBot.Domain.Validations;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Messages.Commands;

public record CreateMessageCommand(
    Guid ConversationId,
    string SenderType,
    string Content) : IRequest<Guid>;

public class CreateMessageCommandValidator : AbstractValidator<CreateMessageCommand>
{
    public CreateMessageCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.ConversationId).NotEmpty();
        RuleFor(c => c.SenderType).NotEmpty().MaximumLength(40);
        RuleFor(c => c.Content).NotEmpty();
    }
}

internal sealed class CreateMessageCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateMessageCommand, Guid>
{
    public async Task<Guid> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.Repository<Conversation>().EnsureExistsAsync("Conversation", request.ConversationId, cancellationToken);

        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderType = TextNormalizer.Required(request.SenderType),
            Content = TextNormalizer.Required(request.Content)
        };

        await _unitOfWork.Messages.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return message.MessageId;
    }
}
