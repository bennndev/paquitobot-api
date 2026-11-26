using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.Messages;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Messages.Queries;


public record GetMessagesByConversationQuery(Guid ConversationId)
    : IRequest<IReadOnlyList<GetMessagesByConversationResponse>>;

public class GetMessagesByConversationQueryValidator : AbstractValidator<GetMessagesByConversationQuery>
{
    public GetMessagesByConversationQueryValidator()
    {
        RuleFor(q => q.ConversationId).NotEmpty();
    }
}

internal sealed class GetMessagesByConversationQueryHandler(IUnitOfWork _unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetMessagesByConversationQuery, IReadOnlyList<GetMessagesByConversationResponse>>
{
    public async Task<IReadOnlyList<GetMessagesByConversationResponse>> Handle(
        GetMessagesByConversationQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await _unitOfWork.Repository<Conversation>()
            .GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation", request.ConversationId);
        ConversationAccess.RequireOwnerOrAdministrator(conversation, currentUser);

        var messages = await _unitOfWork.Messages.GetByConversationAsync(request.ConversationId, cancellationToken);

        return messages
            .Select(m => new GetMessagesByConversationResponse(
                m.MessageId,
                m.SenderType,
                m.Content,
                m.CreatedAt))
            .ToList();
    }
}
