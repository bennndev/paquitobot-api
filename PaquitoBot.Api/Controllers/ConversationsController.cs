using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Conversations.Command;
using PaquitoBot.Application.UseCases.Conversations.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly ISender _sender;

    public ConversationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetAllConversationsResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllConversationsUseCaseQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetConversationByIdResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetConversationByIdUseCaseQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateConversationUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        var conversationId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = conversationId }, conversationId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateConversationUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { ConversationId = id }, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteConversationUseCaseCommand(id), cancellationToken);

        return NoContent();
    }
}
