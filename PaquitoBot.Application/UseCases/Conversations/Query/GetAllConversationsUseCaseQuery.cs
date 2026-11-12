using MediatR;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Query;

public record GetAllConversationsResponse(
    Guid ConversationId,
    Guid UserId,
    string? Title,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record GetAllConversationsUseCaseQuery : IRequest<IReadOnlyList<GetAllConversationsResponse>>;

public class GetAllConversationsUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllConversationsUseCaseQuery, IReadOnlyList<GetAllConversationsResponse>>
{
    public async Task<IReadOnlyList<GetAllConversationsResponse>> Handle(
        GetAllConversationsUseCaseQuery request,
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
