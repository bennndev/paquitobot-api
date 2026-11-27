using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Services.Authenticate;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Services.ChatBot;

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
