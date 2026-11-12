using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Messages.Command;

public record CreateMessageUseCaseCommand(
    Guid ConversationId,
    string SenderType,
    string Content) : IRequest<Guid>;

public class CreateMessageUseCaseCommandValidator : AbstractValidator<CreateMessageUseCaseCommand>
{
    public CreateMessageUseCaseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.ConversationId).NotEmpty();
        RuleFor(c => c.SenderType).NotEmpty().MaximumLength(40);
        RuleFor(c => c.Content).NotEmpty();
    }
}

public class CreateMessageUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateMessageUseCaseCommand, Guid>
{
    public async Task<Guid> Handle(CreateMessageUseCaseCommand request, CancellationToken cancellationToken)
    {
        if (!await _unitOfWork.Repository<Conversation>().ExistsAsync(request.ConversationId, cancellationToken))
        {
            throw new NotFoundException("Conversation", request.ConversationId);
        }

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
