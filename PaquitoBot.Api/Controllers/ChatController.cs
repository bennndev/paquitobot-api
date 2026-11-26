using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Chat.Command;

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

    // Delega la pregunta autenticada al caso de uso que guarda la conversación.
    [HttpPost("ask")]
    public async Task<ActionResult<AskPaquitoResponse>> Ask(
        [FromBody] AskPaquitoCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command, cancellationToken));
    }
}
