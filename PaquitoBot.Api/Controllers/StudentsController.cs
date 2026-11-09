using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Students;
using PaquitoBot.Application.UseCases.Students.Command;
using PaquitoBot.Application.UseCases.Students.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetAll(
        [FromServices] GetAllStudentsUseCaseQuery useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentDto>> GetById(
        Guid id,
        [FromServices] GetStudentByIdUseCaseQuery useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> Create(
        [FromBody] CreateStudentRequest request,
        [FromServices] CreateStudentUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        var student = await useCase.ExecuteAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = student.StudentId }, student);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StudentDto>> Update(
        Guid id,
        [FromBody] UpdateStudentRequest request,
        [FromServices] UpdateStudentUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteStudentUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);

        return NoContent();
    }
}
