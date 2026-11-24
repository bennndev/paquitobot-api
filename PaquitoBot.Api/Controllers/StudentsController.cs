using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.UseCases.Students.Command;
using PaquitoBot.Application.UseCases.Students.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IFrappeEducationClient _frappeClient;

    public StudentsController(ISender sender, IFrappeEducationClient frappeClient)
    {
        _sender = sender;
        _frappeClient = frappeClient;
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

    [HttpGet("frappe/{studentExternalId}")]
    public async Task<ActionResult<StudentDto>> GetFrappeStudent(string studentExternalId, CancellationToken cancellationToken)
    {
        var student = await _frappeClient.GetStudentByExternalIdAsync(studentExternalId, cancellationToken);

        if (student == null)
        {
            return NotFound($"No se encontró el estudiante con external ID: {studentExternalId}");
        }

        return Ok(StudentDto.From(student));
    }

    [HttpGet("frappe")]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetFrappeStudents(CancellationToken cancellationToken)
    {
        var students = await _frappeClient.GetAllStudentsAsync(cancellationToken);

        return Ok(students.Select(StudentDto.From).ToList());
    }
}
