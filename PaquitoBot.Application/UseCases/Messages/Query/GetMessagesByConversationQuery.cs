using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Messages.Query;

public record GetMessagesByConversationResponse(
    Guid MessageId,
    string SenderType,
    string Content,
    DateTime CreatedAt);

public record GetMessagesByConversationQuery(Guid ConversationId)
    : IRequest<IReadOnlyList<GetMessagesByConversationResponse>>;

public class GetMessagesByConversationQueryValidator : AbstractValidator<GetMessagesByConversationQuery>
{
    public GetMessagesByConversationQueryValidator()
    {
        RuleFor(q => q.ConversationId).NotEmpty();
    }
}

public class GetMessagesByConversationQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetMessagesByConversationQuery, IReadOnlyList<GetMessagesByConversationResponse>>
{
    public async Task<IReadOnlyList<GetMessagesByConversationResponse>> Handle(
        GetMessagesByConversationQuery request,
        CancellationToken cancellationToken)
    {
        if (!await _unitOfWork.Repository<Conversation>().ExistsAsync(request.ConversationId, cancellationToken))
        {
            throw new NotFoundException("Conversation", request.ConversationId);
        }

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
