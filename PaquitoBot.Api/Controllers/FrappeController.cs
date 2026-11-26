using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Frappe;
using PaquitoBot.Application.UseCases.Frappe.Command;
using PaquitoBot.Application.UseCases.Frappe.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/frappe")]
public class FrappeController(ISender sender) : ControllerBase
{
    [Authorize(Policy = "AdministratorOrStudent")]
    [HttpGet("connect")]
    public async Task<ActionResult> Connect(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(subject, out var userId)) return Unauthorized();

        var authorization = await sender.Send(new ConnectToFrappeQuery(userId), cancellationToken);
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
