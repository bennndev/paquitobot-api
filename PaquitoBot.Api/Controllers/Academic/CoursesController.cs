using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.Courses;
using PaquitoBot.Application.UseCases.Courses.Commands;
using PaquitoBot.Application.UseCases.Courses.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ISender _sender;

    public CoursesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetMine(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetMyCoursesQuery(), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllCoursesQuery(), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetCourseByIdQuery(id), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateCourseCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Curso creado" });
    }

    [Authorize(Policy = "Administrator")]

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateCourseCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { CourseId = id }, cancellationToken);
        return Ok(new { message = "Curso actualizado" });
    }

    [Authorize(Policy = "Administrator")]

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCourseCommand(id), cancellationToken);

        return Ok(new { message = "Curso eliminado" });
    }
}
