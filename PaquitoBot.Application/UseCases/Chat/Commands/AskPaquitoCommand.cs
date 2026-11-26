using PaquitoBot.Application.Dtos.Chat;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Chat.Commands;

public record AskPaquitoCommand(Guid? ConversationId, string Question) : IRequest<AskPaquitoResponse>;



public class AskPaquitoCommandValidator : AbstractValidator<AskPaquitoCommand>
{
    public AskPaquitoCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.ConversationId).NotEqual(Guid.Empty).When(c => c.ConversationId.HasValue);
        RuleFor(c => c.Question).NotEmpty().MaximumLength(2000);
    }
}

internal sealed class AskPaquitoCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUser _currentUser,
    IKnowledgeRetriever _knowledgeRetriever,
    IAnswerProvider _answerProvider)
    : IRequestHandler<AskPaquitoCommand, AskPaquitoResponse>
{
    private const int MaxTitleLength = 80;
    private const string FailureAnswer = "Sorry, I could not generate an answer right now. Please try again later.";

    public async Task<AskPaquitoResponse> Handle(AskPaquitoCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("The authenticated user could not be identified.");

        var question = TextNormalizer.Required(request.Question);
        var conversation = await GetOrCreateConversationAsync(request.ConversationId, userId, question, cancellationToken);

        var questionMessage = new Message
        {
            Conversation = conversation,
            SenderType = MessageSenderType.User,
            Content = question
        };

        await _unitOfWork.Messages.AddAsync(questionMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        IReadOnlyList<KnowledgeSource> sources = [];
        var succeeded = true;
        string answer;

        try
        {
            sources = await _knowledgeRetriever.RetrieveAsync(question, cancellationToken);
            answer = await _answerProvider.GenerateAnswerAsync(question, sources, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            sources = [];
            answer = FailureAnswer;
            succeeded = false;
        }

        var answerMessage = new Message
        {
            ConversationId = conversation.ConversationId,
            SenderType = MessageSenderType.Assistant,
            Content = MessageSources.Append(answer, sources)
        };

        await _unitOfWork.Messages.AddAsync(answerMessage, cancellationToken);
        conversation.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AskPaquitoResponse(
            conversation.ConversationId,
            questionMessage.MessageId,
            answerMessage.MessageId,
            answer,
            succeeded,
            sources.Select(s => new AskPaquitoSourceResponse(s.DocumentId, s.Title)).ToList());
    }

    private async Task<Conversation> GetOrCreateConversationAsync(
        Guid? conversationId,
        Guid userId,
        string question,
        CancellationToken cancellationToken)
    {
        if (conversationId is { } id)
        {
            var existing = await _unitOfWork.Repository<Conversation>().GetByIdAsync(id, cancellationToken);

            if (existing is null || existing.UserId != userId)
            {
                throw new NotFoundException("Conversation", id);
            }

            return existing;
        }

        var conversation = new Conversation
        {
            UserId = userId,
            Title = question.Length <= MaxTitleLength ? question : question[..MaxTitleLength],
            Status = "active"
        };

        await _unitOfWork.Repository<Conversation>().AddAsync(conversation, cancellationToken);

        return conversation;
    }
}
