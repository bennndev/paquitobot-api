using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.UseCases.Instructors.Command;
using PaquitoBot.Application.UseCases.Instructors.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/instructors")]
public class InstructorsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InstructorDto>>> GetAll(
        [FromServices] GetAllInstructorsUseCaseQuery useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstructorDto>> GetById(
        Guid id,
        [FromServices] GetInstructorByIdUseCaseQuery useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<InstructorDto>> Create(
        [FromBody] CreateInstructorRequest request,
        [FromServices] CreateInstructorUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        var instructor = await useCase.ExecuteAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = instructor.InstructorId }, instructor);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InstructorDto>> Update(
        Guid id,
        [FromBody] UpdateInstructorRequest request,
        [FromServices] UpdateInstructorUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteInstructorUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);

        return NoContent();
    }
}
