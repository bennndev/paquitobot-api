using PaquitoBot.Application.Dtos.Conversations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Conversations.Commands;
using PaquitoBot.Application.UseCases.Conversations.Queries;

namespace PaquitoBot.Api.Controllers.Chat;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly ISender _sender;

    public ConversationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<GetMyConversationsResponse>>> GetMine(
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetMyConversationsQuery(), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetAllConversationsResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllConversationsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetConversationByIdResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetConversationByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateConversationCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Conversación creada" });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateConversationCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { ConversationId = id }, cancellationToken);

        return Ok(new { message = "Conversación actualizada" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteConversationCommand(id), cancellationToken);

        return Ok(new { message = "Conversación eliminada" });
    }
}
