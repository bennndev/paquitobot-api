using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.UseCases.UserStudents.Command;
using PaquitoBot.Application.UseCases.UserStudents.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/user-students")]
public class UserStudentsController : ControllerBase
{
    private readonly ISender _sender;

    public UserStudentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserStudentDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllUserStudentsUseCaseQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserStudentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetUserStudentByIdUseCaseQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UserStudentDto>> Create(
        [FromBody] CreateUserStudentUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        var link = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = link.LinkId }, link);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserStudentDto>> Update(
        Guid id,
        [FromBody] UpdateUserStudentUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { LinkId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteUserStudentUseCaseCommand(id), cancellationToken);

        return NoContent();
    }
}
