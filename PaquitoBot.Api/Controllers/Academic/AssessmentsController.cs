using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Dtos.Assessments;
using PaquitoBot.Application.UseCases.Assessments.Commands;
using PaquitoBot.Application.UseCases.Assessments.Queries;

namespace PaquitoBot.Api.Controllers.Academic;

[ApiController]
[Authorize(Policy = "AdministratorOrStudent")]
[Route("api/assessments")]
public class AssessmentsController(ISender _sender) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<AssessmentDto>>> GetMine(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetMyAssessmentsQuery(), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssessmentDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllAssessmentsQuery(), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssessmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAssessmentByIdQuery(id), cancellationToken));
    }

    [Authorize(Policy = "Administrator")]
    [HttpGet("by-section/{sectionId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AssessmentDto>>> GetBySection(Guid sectionId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAssessmentsBySectionIdQuery(sectionId), cancellationToken));

    [Authorize(Policy = "Administrator")]
    [HttpGet("by-student/{studentId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AssessmentDto>>> GetByStudent(Guid studentId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAssessmentsByStudentIdQuery(studentId), cancellationToken));

    [Authorize(Policy = "Administrator")]
    [HttpGet("by-section/{sectionId:guid}/upcoming")]
    public async Task<ActionResult<IReadOnlyList<AssessmentDto>>> GetUpcomingBySection(Guid sectionId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetUpcomingAssessmentsBySectionIdQuery(sectionId), cancellationToken));

    [Authorize(Policy = "Administrator")]
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return Ok(new { message = "Evaluación creada" });
    }

    [Authorize(Policy = "Administrator")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command with { AssessmentId = id }, cancellationToken);
        return Ok(new { message = "Evaluación actualizada" });
    }

    [Authorize(Policy = "Administrator")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAssessmentCommand(id), cancellationToken);

        return Ok(new { message = "Evaluación eliminada" });
    }
}
