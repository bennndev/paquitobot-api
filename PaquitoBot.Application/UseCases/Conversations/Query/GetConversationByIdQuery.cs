using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Conversations.Query;

public record GetConversationByIdResponse(
    Guid ConversationId,
    Guid UserId,
    string? Title,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record GetConversationByIdQuery(Guid ConversationId) : IRequest<GetConversationByIdResponse>;

public class GetConversationByIdQueryValidator : AbstractValidator<GetConversationByIdQuery>
{
    public GetConversationByIdQueryValidator()
    {
        RuleFor(q => q.ConversationId).NotEmpty();
    }
}

public class GetConversationByIdQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetConversationByIdQuery, GetConversationByIdResponse>
{
    public async Task<GetConversationByIdResponse> Handle(
        GetConversationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await _unitOfWork.Repository<Conversation>().GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation", request.ConversationId);

        return new GetConversationByIdResponse(
            conversation.ConversationId,
            conversation.UserId,
            conversation.Title,
            conversation.Status,
            conversation.CreatedAt,
            conversation.UpdatedAt);
    }
}
