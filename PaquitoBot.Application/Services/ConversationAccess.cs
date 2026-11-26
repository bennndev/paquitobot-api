using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Services;

public static class ConversationAccess
{
    public static void RequireOwnerOrAdministrator(Conversation conversation, ICurrentUser currentUser)
    {
        if (currentUser.IsAdministrator)
        {
            return;
        }

        if (currentUser.UserId != conversation.UserId)
        {
            throw new NotFoundException("Conversation", conversation.ConversationId);
        }
    }
}
