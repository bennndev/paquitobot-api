using MediatR;
using PaquitoBot.Application.DTOs.AttendanceRecords;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.AttendanceRecords.Query;

public record GetMyAttendanceQuery : IRequest<IReadOnlyList<AttendanceRecordDto>>;

internal sealed class GetMyAttendanceQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetMyAttendanceQuery, IReadOnlyList<AttendanceRecordDto>>
{
    public async Task<IReadOnlyList<AttendanceRecordDto>> Handle(GetMyAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        var studentId = currentUser.StudentId
            ?? throw new UnauthorizedAccessException("The current token has no student_id. Sign in again after connecting Frappe.");
        var attendance = await unitOfWork.AttendanceRecords.GetByStudentIdAsync(studentId, cancellationToken);
        return attendance.Select(AttendanceRecordDto.From).ToList();
    }
}
