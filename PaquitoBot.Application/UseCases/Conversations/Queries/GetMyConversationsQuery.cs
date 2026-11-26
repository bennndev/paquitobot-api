using PaquitoBot.Application.Dtos.Conversations;
using MediatR;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.Conversations.Queries;


public record GetMyConversationsQuery : IRequest<IReadOnlyList<GetMyConversationsResponse>>;

internal sealed class GetMyConversationsQueryHandler(IUnitOfWork _unitOfWork, ICurrentUser _currentUser)
    : IRequestHandler<GetMyConversationsQuery, IReadOnlyList<GetMyConversationsResponse>>
{
    public async Task<IReadOnlyList<GetMyConversationsResponse>> Handle(
        GetMyConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("The authenticated user could not be identified.");

        var conversations = await _unitOfWork.Conversations.GetByUserAsync(userId, cancellationToken);

        return conversations
            .Select(c => new GetMyConversationsResponse(
                c.ConversationId,
                c.Title,
                c.Status,
                c.CreatedAt,
                c.UpdatedAt))
            .ToList();
    }
}
