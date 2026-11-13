using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Courses;
using PaquitoBot.Application.UseCases.Courses.Command;
using PaquitoBot.Application.UseCases.Courses.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ISender _sender;

    public CoursesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllCoursesQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetCourseByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<CourseDto>> Create(
        [FromBody] CreateCourseCommand command,
        CancellationToken cancellationToken)
    {
        var course = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = course.CourseId }, course);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CourseDto>> Update(
        Guid id,
        [FromBody] UpdateCourseCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { CourseId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCourseCommand(id), cancellationToken);

        return NoContent();
    }
}
