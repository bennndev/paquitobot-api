using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Users;
using PaquitoBot.Application.UseCases.Users.Command;
using PaquitoBot.Application.UseCases.Users.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(
        [FromServices] GetAllUsersUseCaseQuery useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(
        Guid id,
        [FromServices] GetUserByIdUseCaseQuery useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        [FromBody] CreateUserRequest request,
        [FromServices] CreateUserUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        var user = await useCase.ExecuteAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(
        Guid id,
        [FromBody] UpdateUserRequest request,
        [FromServices] UpdateUserUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteUserUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);

        return NoContent();
    }
}
