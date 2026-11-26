using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.UserStudents;
using PaquitoBot.Application.UseCases.UserStudents.Commands;
using PaquitoBot.Application.UseCases.UserStudents.Queries;

namespace PaquitoBot.Api.Controllers.Users;

[ApiController]
[Authorize(Policy = "Administrator")]
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
        return Ok(await _sender.Send(new GetAllUserStudentsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserStudentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetUserStudentByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateUserStudentCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Vínculo entre usuario y estudiante creado" });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateUserStudentCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { LinkId = id }, cancellationToken);
        return Ok(new { message = "Vínculo entre usuario y estudiante actualizado" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteUserStudentCommand(id), cancellationToken);

        return Ok(new { message = "Vínculo entre usuario y estudiante eliminado" });
    }
}
