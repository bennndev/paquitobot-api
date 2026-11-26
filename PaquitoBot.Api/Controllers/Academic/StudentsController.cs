using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Application.Dtos.Students;
using PaquitoBot.Application.UseCases.Students.Commands;
using PaquitoBot.Application.UseCases.Students.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "Administrator")]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly ISender _sender;

    public StudentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllStudentsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetStudentByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Estudiante creado" });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateStudentCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { StudentId = id }, cancellationToken);
        return Ok(new { message = "Estudiante actualizado" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteStudentCommand(id), cancellationToken);

        return Ok(new { message = "Estudiante eliminado" });
    }

    [HttpGet("frappe")]
    public async Task<ActionResult<IReadOnlyList<FrappeStudentDto>>> GetFrappeStudents(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetFrappeStudentsQuery(), cancellationToken));
    }
}
