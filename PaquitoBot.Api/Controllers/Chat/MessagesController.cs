using PaquitoBot.Application.Dtos.Messages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Messages.Commands;
using PaquitoBot.Application.UseCases.Messages.Queries;

namespace PaquitoBot.Api.Controllers.Chat;

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
    public async Task<ActionResult> Create(
        [FromBody] CreateMessageCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Mensaje creado" });
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
        return Ok(new { message = "Mensaje eliminado" });
    }
}
