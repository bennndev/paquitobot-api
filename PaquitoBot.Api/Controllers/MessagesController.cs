using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Messages.Command;
using PaquitoBot.Application.UseCases.Messages.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
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
        return Ok(await _sender.Send(new GetMessagesByConversationUseCaseQuery(conversationId), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetMessageByIdResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetMessageByIdUseCaseQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateMessageUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        var messageId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = messageId }, messageId);
    }
}
