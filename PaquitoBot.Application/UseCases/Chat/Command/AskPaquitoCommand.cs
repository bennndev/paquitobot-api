using System.Text.Json;
using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.UseCases.StudentFinances.Query;
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
    ISender _sender)
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

        await _unitOfWork.Messages.AddAsync(questionMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
                if (modelResponse.ToolRequest.Name != "get_my_courses" &&
                    modelResponse.ToolRequest.Name != "get_my_assessments" &&
                    modelResponse.ToolRequest.Name != "get_my_grades" &&
                    modelResponse.ToolRequest.Name != "get_my_attendance" &&
                    modelResponse.ToolRequest.Name != "get_my_finances")
                {
                    throw new InvalidOperationException("Gemini requested an unsupported tool.");
                }

                string toolResult;
                if (_currentUser.StudentId is not Guid studentId ||
                    !await _unitOfWork.UserStudents.PairExistsAsync(userId, studentId, cancellationToken))
                {
                    toolResult = JsonSerializer.Serialize(new { error = "student_account_not_connected" });
                }
                else
                {
                    if (modelResponse.ToolRequest.Name == "get_my_courses")
                    {
                        var courses = await _unitOfWork.Courses.GetByStudentIdAsync(studentId, cancellationToken);
                        toolResult = JsonSerializer.Serialize(courses.Select(course => new
                        {
                            course.Code,
                            course.Name,
                            course.Status
                        }));
                    }
                    else if (modelResponse.ToolRequest.Name == "get_my_assessments")
                    {
                        var assessments = await _unitOfWork.Assessments.GetByStudentIdAsync(studentId, cancellationToken);
                        toolResult = JsonSerializer.Serialize(assessments.Select(assessment => new
                        {
                            assessment.Title,
                            assessment.AssessmentType,
                            assessment.AssessmentDate,
                            assessment.MaximumScore,
                            assessment.Status
                        }));
                    }
                    else if (modelResponse.ToolRequest.Name == "get_my_grades")
                    {
                        var results = await _unitOfWork.AssessmentResults.GetByStudentIdAsync(studentId, cancellationToken);
                        var latestResults = results.GroupBy(result => result.AssessmentId)
                            .Select(group => group.OrderByDescending(result => result.UpdatedAt)
                                .ThenByDescending(result => result.CreatedAt)
                                .ThenByDescending(result => result.ResultId)
                                .First());
                        toolResult = JsonSerializer.Serialize(latestResults.Select(result => new
                        {
                            result.Assessment.Title,
                            result.Assessment.AssessmentDate,
                            result.Grade,
                            result.Assessment.MaximumScore,
                            result.Notes
                        }));
                    }
                    else if (modelResponse.ToolRequest.Name == "get_my_attendance")
                    {
                        var records = await _unitOfWork.AttendanceRecords.GetByStudentIdAsync(studentId, cancellationToken);
                        toolResult = JsonSerializer.Serialize(records.Select(record => new
                        {
                            Course = record.Section.Course.Name,
                            record.Section.SectionCode,
                            record.AttendanceDate,
                            record.Status,
                            record.Notes
                        }));
                    }
                    else
                    {
                        var statement = await _sender.Send(
                            new GetStudentFinancialStatementQuery(studentId, userId), cancellationToken);
                        toolResult = JsonSerializer.Serialize(new
                        {
                            statement.AsOf,
                            statement.Currencies,
                            Obligations = statement.Obligations.Select(obligation => new
                            {
                                obligation.Description,
                                obligation.CurrencyCode,
                                obligation.OriginalAmount,
                                obligation.ConfirmedPaidAmount,
                                obligation.OutstandingAmount,
                                obligation.DueDate,
                                obligation.CalculatedStatus,
                                obligation.ReconciliationRequired
                            }),
                            statement.PaymentsNotCounted
                        });
                    }
                }

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
