using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.UseCases.Students.Command;
using PaquitoBot.Application.UseCases.Students.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
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
    public async Task<ActionResult<StudentDto>> Create(
        [FromBody] CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        var student = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = student.StudentId }, student);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StudentDto>> Update(
        Guid id,
        [FromBody] UpdateStudentCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { StudentId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteStudentCommand(id), cancellationToken);

        return NoContent();
    }
}
