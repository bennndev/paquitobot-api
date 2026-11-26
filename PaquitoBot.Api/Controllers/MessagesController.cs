using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Messages.Command;
using PaquitoBot.Application.UseCases.Messages.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly ISender _sender;

    public MessagesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("~/api/conversations/{conversationId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<GetMessagesByConversationResponse>>> GetByConversation(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetMessagesByConversationQuery(conversationId), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetMessageByIdResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetMessageByIdQuery(id), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateMessageCommand command,
        CancellationToken cancellationToken)
    {
        var messageId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = messageId }, messageId);
    }

    [HttpGet("conversations/{conversationId:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<GetChatHistoryResponse>>> GetHistory(
        Guid conversationId, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetChatHistoryQuery(conversationId), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteMessageCommand(id), cancellationToken);
        return NoContent();
    }
}
