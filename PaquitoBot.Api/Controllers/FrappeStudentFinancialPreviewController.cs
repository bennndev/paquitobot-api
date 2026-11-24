using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.StudentFinances;
using PaquitoBot.Application.UseCases.StudentFinances.Command;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Authorize(Policy = "Administrator")]
[Route("api/student-finances")]
[Produces("application/json")]
public sealed class FrappeStudentFinancialPreviewController(ISender sender) : ControllerBase
{
    [HttpPost("{studentId:guid}/frappe/preview")]
    [ProducesResponseType(typeof(FrappeFinancialPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FrappeFinancialPreviewDto>> Preview(
        Guid studentId,
        [FromQuery] string source = "Fees",
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new PreviewFrappeStudentFeesCommand(studentId, source),
            cancellationToken);

        return Ok(result);
    }
}
