using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.AttendanceRecords;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AttendanceRecords.Query;

public record GetAttendanceRecordByIdQuery(Guid AttendanceId) : IRequest<AttendanceRecordDto>;

internal sealed class GetAttendanceRecordByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAttendanceRecordByIdQuery, AttendanceRecordDto>
{
    public async Task<AttendanceRecordDto> Handle(
        GetAttendanceRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var attendance = await unitOfWork.Repository<AttendanceRecord>().GetByIdAsync(request.AttendanceId, cancellationToken)
            ?? throw new NotFoundException("AttendanceRecord", request.AttendanceId);

        return AttendanceRecordDto.From(attendance);
    }
}
