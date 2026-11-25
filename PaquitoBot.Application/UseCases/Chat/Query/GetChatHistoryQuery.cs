using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Chat.Query;

public record GetChatHistorySourceResponse(Guid DocumentId, string Title);

public record GetChatHistoryResponse(
    Guid MessageId,
    string SenderType,
    string Content,
    IReadOnlyList<GetChatHistorySourceResponse> Sources,
    DateTime CreatedAt);

public record GetChatHistoryQuery(Guid ConversationId) : IRequest<IReadOnlyList<GetChatHistoryResponse>>;

public class GetChatHistoryQueryValidator : AbstractValidator<GetChatHistoryQuery>
{
    public GetChatHistoryQueryValidator()
    {
        RuleFor(q => q.ConversationId).NotEmpty();
    }
}

public class GetChatHistoryQueryHandler(IUnitOfWork _unitOfWork, ICurrentUser _currentUser)
    : IRequestHandler<GetChatHistoryQuery, IReadOnlyList<GetChatHistoryResponse>>
{
    public async Task<IReadOnlyList<GetChatHistoryResponse>> Handle(
        GetChatHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("The authenticated user could not be identified.");

        var conversation = await _unitOfWork.Repository<Conversation>()
            .GetByIdAsync(request.ConversationId, cancellationToken);

        if (conversation is null || conversation.UserId != userId)
        {
            throw new NotFoundException("Conversation", request.ConversationId);
        }

        var messages = await _unitOfWork.Messages.GetByConversationAsync(request.ConversationId, cancellationToken);

        return messages
            .Select(m =>
            {
                var (content, sources) = MessageSources.Split(m.Content);

                return new GetChatHistoryResponse(
                    m.MessageId,
                    m.SenderType,
                    content,
                    sources.Select(s => new GetChatHistorySourceResponse(s.DocumentId, s.Title)).ToList(),
                    m.CreatedAt);
            })
            .ToList();
    }
}
