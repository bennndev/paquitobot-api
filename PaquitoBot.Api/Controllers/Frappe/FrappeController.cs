using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Frappe;
using PaquitoBot.Application.UseCases.Frappe.Commands;
using PaquitoBot.Application.UseCases.Frappe.Queries;

namespace PaquitoBot.Api.Controllers.Frappe;

[ApiController]
[Route("api/frappe")]
public class FrappeController(ISender sender) : ControllerBase
{
    [Authorize(Policy = "AdministratorOrStudent")]
    [HttpGet("connect")]
    public async Task<ActionResult> Connect(CancellationToken cancellationToken)
    {
        var authorization = await sender.Send(new ConnectToFrappeQuery(), cancellationToken);
        return Ok(authorization);
    }

    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task<ActionResult> Callback([FromQuery] string? code, [FromQuery] string? state,
        [FromQuery] string? error, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error)) return BadRequest(new { error });
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            return BadRequest(new { error = "Missing OAuth code or state." });

        var result = await sender.Send(new CompleteFrappeConnectionCommand(code, state), cancellationToken);
        return Ok(result);
    }
}
