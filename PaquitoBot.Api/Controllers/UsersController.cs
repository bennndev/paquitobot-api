using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.UseCases.Users.Command;
using PaquitoBot.Application.UseCases.Users.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllUsersUseCaseQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetUserByIdUseCaseQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        [FromBody] CreateUserUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(
        Guid id,
        [FromBody] UpdateUserUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { UserId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteUserUseCaseCommand(id), cancellationToken);

        return NoContent();
    }
}
