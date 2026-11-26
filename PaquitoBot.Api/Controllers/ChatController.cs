using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Chat.Command;
using PaquitoBot.Application.UseCases.Chat.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ISender _sender;

    public ChatController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<AskPaquitoResponse>> Ask(
        [FromBody] AskPaquitoCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<IReadOnlyList<GetMyConversationsResponse>>> GetMyConversations(
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetMyConversationsQuery(), cancellationToken));
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<GetChatHistoryResponse>>> GetHistory(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetChatHistoryQuery(conversationId), cancellationToken));
    }
}
