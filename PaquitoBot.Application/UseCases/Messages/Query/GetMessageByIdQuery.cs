using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Authenticate;
using PaquitoBot.Application.Services.ChatBot;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Messages.Query;

public record GetMessageByIdResponse(
    Guid MessageId,
    Guid ConversationId,
    string SenderType,
    string Content,
    DateTime CreatedAt);

public record GetMessageByIdQuery(Guid MessageId) : IRequest<GetMessageByIdResponse>;

public class GetMessageByIdQueryValidator : AbstractValidator<GetMessageByIdQuery>
{
    public GetMessageByIdQueryValidator()
    {
        RuleFor(q => q.MessageId).NotEmpty();
    }
}

public class GetMessageByIdQueryHandler(IUnitOfWork _unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetMessageByIdQuery, GetMessageByIdResponse>
{
    public async Task<GetMessageByIdResponse> Handle(
        GetMessageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var message = await _unitOfWork.Messages.GetByIdAsync(request.MessageId, cancellationToken)
            ?? throw new NotFoundException("Message", request.MessageId);

        var conversation = await _unitOfWork.Repository<Conversation>()
            .GetByIdAsync(message.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Message", request.MessageId);
        ConversationAccess.RequireOwnerOrAdministrator(conversation, currentUser);

        return new GetMessageByIdResponse(
            message.MessageId,
            message.ConversationId,
            message.SenderType,
            message.Content,
            message.CreatedAt);
    }
}
