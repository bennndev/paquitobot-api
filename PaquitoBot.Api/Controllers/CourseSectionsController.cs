using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.CourseSections;
using PaquitoBot.Application.UseCases.CourseSections.Command;
using PaquitoBot.Application.UseCases.CourseSections.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/course-sections")]
public class CourseSectionsController : ControllerBase
{
    private readonly ISender _sender;

    public CourseSectionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseSectionDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllCourseSectionsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseSectionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetCourseSectionByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<CourseSectionDto>> Create(
        [FromBody] CreateCourseSectionCommand command,
        CancellationToken cancellationToken)
    {
        var section = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = section.SectionId }, section);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CourseSectionDto>> Update(
        Guid id,
        [FromBody] UpdateCourseSectionCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { SectionId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCourseSectionCommand(id), cancellationToken);

        return NoContent();
    }
}
