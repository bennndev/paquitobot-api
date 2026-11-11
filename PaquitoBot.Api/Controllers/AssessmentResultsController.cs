using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.AssessmentsResults;
using PaquitoBot.Application.UseCases.AssessmentsResults.Command;
using PaquitoBot.Application.UseCases.AssessmentsResults.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/assessment-results")]
public class AssessmentResultsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssessmentResultDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetAllAssessmentResultsUseCaseQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssessmentResultDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetAssessmentResultByIdUseCaseQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateAssessmentResultUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        var resultId = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = resultId }, resultId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Guid>> Update(
        Guid id,
        [FromBody] UpdateAssessmentResultUseCaseCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(command with { ResultId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAssessmentResultUseCaseCommand(id), cancellationToken);
        return NoContent();
    }
}
