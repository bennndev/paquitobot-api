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

public record GetConversationByIdUseCaseQuery(Guid ConversationId) : IRequest<GetConversationByIdResponse>;

public class GetConversationByIdUseCaseQueryValidator : AbstractValidator<GetConversationByIdUseCaseQuery>
{
    public GetConversationByIdUseCaseQueryValidator()
    {
        RuleFor(q => q.ConversationId).NotEmpty();
    }
}

public class GetConversationByIdUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetConversationByIdUseCaseQuery, GetConversationByIdResponse>
{
    public async Task<GetConversationByIdResponse> Handle(
        GetConversationByIdUseCaseQuery request,
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
