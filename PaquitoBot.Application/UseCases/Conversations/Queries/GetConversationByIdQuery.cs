using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.Conversations;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Queries;


public record GetConversationByIdQuery(Guid ConversationId) : IRequest<GetConversationByIdResponse>;

public class GetConversationByIdQueryValidator : AbstractValidator<GetConversationByIdQuery>
{
    public GetConversationByIdQueryValidator()
    {
        RuleFor(q => q.ConversationId).NotEmpty();
    }
}

internal sealed class GetConversationByIdQueryHandler(IUnitOfWork _unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetConversationByIdQuery, GetConversationByIdResponse>
{
    public async Task<GetConversationByIdResponse> Handle(
        GetConversationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await _unitOfWork.Repository<Conversation>().GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation", request.ConversationId);

        ConversationAccess.RequireOwnerOrAdministrator(conversation, currentUser);

        return new GetConversationByIdResponse(
            conversation.ConversationId,
            conversation.UserId,
            conversation.Title,
            conversation.Status,
            conversation.CreatedAt,
            conversation.UpdatedAt);
    }
}
