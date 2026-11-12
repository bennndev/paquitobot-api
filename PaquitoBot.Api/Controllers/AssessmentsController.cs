using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.Assessments;
using PaquitoBot.Application.UseCases.Assessments.Command;
using PaquitoBot.Application.UseCases.Assessments.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/assessments")]
public class AssessmentsController(ISender _sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssessmentDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllAssessmentsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssessmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAssessmentByIdQuery(id), cancellationToken));
    }

    [HttpGet("by-section/{sectionId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AssessmentDto>>> GetBySection(Guid sectionId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAssessmentsBySectionIdQuery(sectionId), cancellationToken));

    [HttpGet("by-student/{studentId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AssessmentDto>>> GetByStudent(Guid studentId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAssessmentsByStudentIdQuery(studentId), cancellationToken));

    [HttpGet("by-section/{sectionId:guid}/upcoming")]
    public async Task<ActionResult<IReadOnlyList<AssessmentDto>>> GetUpcomingBySection(Guid sectionId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetUpcomingAssessmentsBySectionIdQuery(sectionId), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        var assessmentId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = assessmentId }, assessmentId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Guid>> Update(
        Guid id,
        [FromBody] UpdateAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { AssessmentId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAssessmentCommand(id), cancellationToken);

        return NoContent();
    }
}
