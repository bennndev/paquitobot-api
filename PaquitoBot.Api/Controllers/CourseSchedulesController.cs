using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.CourseSchedules;
using PaquitoBot.Application.UseCases.CourseSchedules.Command;
using PaquitoBot.Application.UseCases.CourseSchedules.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/course-schedules")]
public class CourseSchedulesController : ControllerBase
{
    private readonly ISender _sender;

    public CourseSchedulesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseScheduleDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllCourseSchedulesQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseScheduleDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetCourseScheduleByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<CourseScheduleDto>> Create(
        [FromBody] CreateCourseScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var schedule = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = schedule.ScheduleId }, schedule);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CourseScheduleDto>> Update(
        Guid id,
        [FromBody] UpdateCourseScheduleCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { ScheduleId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCourseScheduleCommand(id), cancellationToken);

        return NoContent();
    }
}
