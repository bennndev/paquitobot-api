using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.CourseSections;
using PaquitoBot.Application.UseCases.CourseSections.Commands;
using PaquitoBot.Application.UseCases.CourseSections.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "Administrator")]
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
    public async Task<ActionResult> Create(
        [FromBody] CreateCourseSectionCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Sección creada" });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateCourseSectionCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { SectionId = id }, cancellationToken);
        return Ok(new { message = "Sección actualizada" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCourseSectionCommand(id), cancellationToken);

        return Ok(new { message = "Sección eliminada" });
    }
}
