using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.UseCases.Chat.Command;
using PaquitoBot.Application.UseCases.Chat.Query;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;
using Xunit;

namespace PaquitoBot.Infrastructure.Tests.Chat;

public sealed class ChatTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly KnowledgeSource Source = new(Guid.NewGuid(), "Enrollment guide", "How to enroll");

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static AskPaquitoCommandHandler CreateAskHandler(
        FakeUnitOfWork unitOfWork,
        Guid? userId,
        IReadOnlyList<KnowledgeSource>? sources = null,
        bool retrieverFails = false,
        bool providerFails = false)
    {
        return new AskPaquitoCommandHandler(
            unitOfWork,
            new StubCurrentUser(userId),
            new StubRetriever(sources ?? [], retrieverFails),
            new StubAnswerProvider(providerFails));
    }

    [Fact]
    public async Task AskCreatesConversationAndStoresQuestionAndAnswerWithSources()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateAskHandler(unitOfWork, UserId, [Source]);

        var response = await handler.Handle(new AskPaquitoCommand(null, "  How do I enroll?  "), Token);

        Assert.True(response.Succeeded);
        Assert.Equal("answer to: How do I enroll?", response.Answer);
        Assert.Equal(Source.DocumentId, Assert.Single(response.Sources).DocumentId);

        var conversation = Assert.Single(unitOfWork.StoredConversations);
        Assert.Equal(UserId, conversation.UserId);
        Assert.Equal(response.ConversationId, conversation.ConversationId);
        Assert.Equal(2, unitOfWork.StoredMessages.Count);

        var question = unitOfWork.StoredMessages.Single(m => m.MessageId == response.QuestionMessageId);
        var answer = unitOfWork.StoredMessages.Single(m => m.MessageId == response.AnswerMessageId);
        Assert.Equal(MessageSenderType.User, question.SenderType);
        Assert.Equal("How do I enroll?", question.Content);
        Assert.Equal(MessageSenderType.Assistant, answer.SenderType);
        Assert.Contains(Source.DocumentId.ToString(), answer.Content);
    }

    [Fact]
    public async Task AskWithoutContextAnswersWithoutSources()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateAskHandler(unitOfWork, UserId);

        var response = await handler.Handle(new AskPaquitoCommand(null, "Unknown topic"), Token);

        Assert.True(response.Succeeded);
        Assert.Empty(response.Sources);
        var answer = unitOfWork.StoredMessages.Single(m => m.MessageId == response.AnswerMessageId);
        Assert.Equal("answer to: Unknown topic", answer.Content);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task AskKeepsConversationConsistentWhenAProviderFails(bool retrieverFails, bool providerFails)
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateAskHandler(unitOfWork, UserId, [Source], retrieverFails, providerFails);

        var response = await handler.Handle(new AskPaquitoCommand(null, "How do I enroll?"), Token);

        Assert.False(response.Succeeded);
        Assert.Empty(response.Sources);
        Assert.Equal(2, unitOfWork.StoredMessages.Count);
        Assert.Contains(unitOfWork.StoredMessages, m => m.SenderType == MessageSenderType.User);
        Assert.Contains(unitOfWork.StoredMessages, m => m.SenderType == MessageSenderType.Assistant);
        Assert.Single(unitOfWork.StoredConversations);
    }

    [Fact]
    public async Task AskDeniesAccessToAnotherUsersConversation()
    {
        var unitOfWork = new FakeUnitOfWork();
        var owner = CreateAskHandler(unitOfWork, Guid.NewGuid());
        var created = await owner.Handle(new AskPaquitoCommand(null, "Private question"), Token);
        var intruder = CreateAskHandler(unitOfWork, UserId);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            intruder.Handle(new AskPaquitoCommand(created.ConversationId, "Hello"), Token));

        Assert.Equal(2, unitOfWork.StoredMessages.Count);
    }

    [Fact]
    public async Task AskRequiresAnAuthenticatedUser()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateAskHandler(unitOfWork, null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new AskPaquitoCommand(null, "Hello"), Token));

        Assert.Empty(unitOfWork.StoredConversations);
        Assert.Empty(unitOfWork.StoredMessages);
    }

    [Fact]
    public async Task HistoryReturnsMessagesWithSourcesOnlyToTheOwner()
    {
        var unitOfWork = new FakeUnitOfWork();
        var asked = await CreateAskHandler(unitOfWork, UserId, [Source])
            .Handle(new AskPaquitoCommand(null, "How do I enroll?"), Token);

        var history = await new GetChatHistoryQueryHandler(unitOfWork, new StubCurrentUser(UserId))
            .Handle(new GetChatHistoryQuery(asked.ConversationId), Token);

        Assert.Equal(2, history.Count);
        var answer = history.Single(m => m.SenderType == MessageSenderType.Assistant);
        Assert.Equal("answer to: How do I enroll?", answer.Content);
        Assert.Equal(Source.DocumentId, Assert.Single(answer.Sources).DocumentId);
        Assert.Empty(history.Single(m => m.SenderType == MessageSenderType.User).Sources);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetChatHistoryQueryHandler(unitOfWork, new StubCurrentUser(Guid.NewGuid()))
                .Handle(new GetChatHistoryQuery(asked.ConversationId), Token));
    }

    [Fact]
    public async Task MyConversationsListsOnlyTheCurrentUsersConversations()
    {
        var unitOfWork = new FakeUnitOfWork();
        await CreateAskHandler(unitOfWork, UserId).Handle(new AskPaquitoCommand(null, "Mine"), Token);
        await CreateAskHandler(unitOfWork, Guid.NewGuid()).Handle(new AskPaquitoCommand(null, "Theirs"), Token);

        var conversations = await new GetMyConversationsQueryHandler(unitOfWork, new StubCurrentUser(UserId))
            .Handle(new GetMyConversationsQuery(), Token);

        Assert.Equal("Mine", Assert.Single(conversations).Title);
    }

    [Fact]
    public void MessageSourcesRoundTripAndIgnoreMessagesWithoutSources()
    {
        var content = MessageSources.Append("The answer", [Source]);
        var (answer, sources) = MessageSources.Split(content);

        Assert.Equal("The answer", answer);
        Assert.Equal(new MessageSource(Source.DocumentId, Source.Title), Assert.Single(sources));

        var (plain, none) = MessageSources.Split("Just text");
        Assert.Equal("Just text", plain);
        Assert.Empty(none);
    }
}
