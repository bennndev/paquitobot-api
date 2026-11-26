using PaquitoBot.Application.Dtos.Conversations;
using MediatR;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Queries;


public record GetAllConversationsQuery : IRequest<IReadOnlyList<GetAllConversationsResponse>>;

internal sealed class GetAllConversationsQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllConversationsQuery, IReadOnlyList<GetAllConversationsResponse>>
{
    public async Task<IReadOnlyList<GetAllConversationsResponse>> Handle(
        GetAllConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var conversations = await _unitOfWork.Repository<Conversation>().GetAllAsync(cancellationToken);

        return conversations
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new GetAllConversationsResponse(
                c.ConversationId,
                c.UserId,
                c.Title,
                c.Status,
                c.CreatedAt,
                c.UpdatedAt))
            .ToList();
    }
}
