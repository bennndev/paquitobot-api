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
        return Ok(await _sender.Send(new GetAllAssessmentsUseCaseQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssessmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAssessmentByIdUseCaseQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateAssessmentUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        var assessmentId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = assessmentId }, assessmentId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Guid>> Update(
        Guid id,
        [FromBody] UpdateAssessmentUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(command with { AssessmentId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAssessmentUseCaseCommand(id), cancellationToken);

        return NoContent();
    }
}
