using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.UseCases.Auth.Commands;

namespace PaquitoBot.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender sender) : ControllerBase
{

    [Authorize(Policy = "Administrator")]
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("admin-example");
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] AuthenticateCommand request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(request,  cancellationToken));
    }

    [HttpPost("register-user")]
    public async Task<ActionResult> Register([FromBody] RegisterCommand request, CancellationToken cancellationToken)
    {
        var userId = await sender.Send(request, cancellationToken);
        return Ok(new { message = "Usuario registrado", userId });
    }
}
