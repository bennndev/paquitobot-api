using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PaquitoBot.Application.UseCases.Auth.Command;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("password-reset")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        if (await sender.Send(request, cancellationToken)) return Accepted();
        return Problem(statusCode: 503, title: "Password recovery is unavailable.");
    }

    [AllowAnonymous]
    [EnableRateLimiting("password-reset")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (await sender.Send(request, cancellationToken)) return NoContent();
        return Problem(statusCode: 400, title: "The password reset link is invalid or expired.");
    }

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
        return Ok(await sender.Send(request,  cancellationToken));
    }
}
