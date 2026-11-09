using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.UserStudents;
using PaquitoBot.Application.UseCases.UserStudents.Command;
using PaquitoBot.Application.UseCases.UserStudents.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/user-students")]
public class UserStudentsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserStudentDto>>> GetAll(
        [FromServices] GetAllUserStudentsUseCaseQuery useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserStudentDto>> GetById(
        Guid id,
        [FromServices] GetUserStudentByIdUseCaseQuery useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UserStudentDto>> Create(
        [FromBody] CreateUserStudentRequest request,
        [FromServices] CreateUserStudentUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        var link = await useCase.ExecuteAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = link.LinkId }, link);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserStudentDto>> Update(
        Guid id,
        [FromBody] UpdateUserStudentRequest request,
        [FromServices] UpdateUserStudentUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteUserStudentUseCaseCommand useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);

        return NoContent();
    }
}
