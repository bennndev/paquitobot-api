using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.CourseSchedules;
using PaquitoBot.Application.UseCases.CourseSchedules.Commands;
using PaquitoBot.Application.UseCases.CourseSchedules.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "Administrator")]
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
    public async Task<ActionResult> Create(
        [FromBody] CreateCourseScheduleCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Horario creado" });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateCourseScheduleCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { ScheduleId = id }, cancellationToken);
        return Ok(new { message = "Horario actualizado" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCourseScheduleCommand(id), cancellationToken);

        return Ok(new { message = "Horario eliminado" });
    }
}
