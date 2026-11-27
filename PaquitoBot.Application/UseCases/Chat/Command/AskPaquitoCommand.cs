using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Chat;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Authenticate;
using PaquitoBot.Application.Services.ChatBot;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Chat.Command;

public record AskPaquitoCommand(Guid? ConversationId, string Question) : IRequest<AskPaquitoResponse>;

public record AskPaquitoSourceResponse(Guid DocumentId, string Title);

public record AskPaquitoResponse(
    Guid ConversationId,
    Guid QuestionMessageId,
    Guid AnswerMessageId,
    string Answer,
    bool Succeeded,
    IReadOnlyList<AskPaquitoSourceResponse> Sources);

public class AskPaquitoCommandValidator : AbstractValidator<AskPaquitoCommand>
{
    public AskPaquitoCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.ConversationId).NotEqual(Guid.Empty).When(c => c.ConversationId.HasValue);
        RuleFor(c => c.Question).NotEmpty().MaximumLength(2000);
    }
}

public class AskPaquitoCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUser _currentUser,
    IAnswerProvider _answerProvider,
    IChatToolExecutor _toolExecutor)
    : IRequestHandler<AskPaquitoCommand, AskPaquitoResponse>
{
    private const int MaxTitleLength = 80;
    private const int MaxContextMessages = 10;
    private const int MaxContextMessageLength = 2000;
    private const string FailureAnswer = "No pude generar una respuesta ahora. Inténtalo de nuevo más tarde.";

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

        // Guarda la pregunta incluso si Gemini falla después.
        await _unitOfWork.Messages.AddAsync(questionMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Limita el historial enviado al modelo para evitar un contexto sin fin.
        var recentMessages = await _unitOfWork.Messages.GetRecentByConversationAsync(
            conversation.ConversationId, MaxContextMessages, cancellationToken);
        var turns = new List<ChatTurn>();
        foreach (var message in recentMessages)
        {
            if (message.SenderType != MessageSenderType.User && message.SenderType != MessageSenderType.Assistant)
            {
                continue;
            }

            var (content, _) = MessageSources.Split(message.Content);
            if (content.Length > MaxContextMessageLength)
            {
                content = content[..MaxContextMessageLength];
            }

            turns.Add(new ChatTurn(message.SenderType, content));
        }

        var succeeded = true;
        string answer;

        try
        {
            var modelResponse = await _answerProvider.BeginAnswerAsync(turns, cancellationToken);
            if (modelResponse.ToolRequest is null)
            {
                answer = modelResponse.Answer
                    ?? throw new InvalidOperationException("Gemini returned no answer.");
            }
            else
            {
                // El ejecutor decide qué datos del estudiante autenticado puede consultar la herramienta.
                var toolResult = await _toolExecutor.ExecuteAsync(modelResponse.ToolRequest.Name, cancellationToken);
                answer = await _answerProvider.CompleteToolAsync(
                    modelResponse.ToolRequest, toolResult, cancellationToken);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            answer = FailureAnswer;
            succeeded = false;
        }

        var answerMessage = new Message
        {
            ConversationId = conversation.ConversationId,
            SenderType = MessageSenderType.Assistant,
            Content = answer
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
            []);
    }

    // Reutiliza solo conversaciones del usuario; crea una si no se indicó identificador.
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
