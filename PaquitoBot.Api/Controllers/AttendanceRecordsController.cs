using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.DTOs.AttendanceRecords;
using PaquitoBot.Application.UseCases.AttendanceRecords.Command;
using PaquitoBot.Application.UseCases.AttendanceRecords.Query;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/attendance-records")]
public class AttendanceRecordsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetAllAttendanceRecordsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AttendanceRecordDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetAttendanceRecordByIdQuery(id), cancellationToken));
    }

    [HttpGet("by-student/{studentId:guid}/section/{sectionId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> GetByStudentAndSection(
        Guid studentId, Guid sectionId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAttendanceByStudentAndSectionQuery(studentId, sectionId), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateAttendanceRecordCommand command,
        CancellationToken cancellationToken)
    {
        var attendanceId = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = attendanceId }, attendanceId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Guid>> Update(
        Guid id,
        [FromBody] UpdateAttendanceRecordCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(command with { AttendanceId = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAttendanceRecordCommand(id), cancellationToken);
        return NoContent();
    }
}
