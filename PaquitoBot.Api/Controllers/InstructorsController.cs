using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Instructors;
using PaquitoBot.Application.UseCases.Instructors.Command;
using PaquitoBot.Application.UseCases.Instructors.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/instructors")]
public class InstructorsController : ControllerBase
{
    private readonly ISender _sender;

    public InstructorsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InstructorDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllInstructorsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstructorDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetInstructorByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<InstructorDto>> Create(
        [FromBody] CreateInstructorCommand command,
        CancellationToken cancellationToken)
    {
        var instructor = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = instructor.InstructorId }, instructor);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InstructorDto>> Update(
        Guid id,
        [FromBody] UpdateInstructorCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { InstructorId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteInstructorCommand(id), cancellationToken);

        return NoContent();
    }
}
