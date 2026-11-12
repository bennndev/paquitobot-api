using MediatR;
using PaquitoBot.Application.DTOs.AttendanceRecords;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AttendanceRecords.Query;

public record GetAllAttendanceRecordsQuery : IRequest<IReadOnlyList<AttendanceRecordDto>>;

internal sealed class GetAllAttendanceRecordsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAttendanceRecordsQuery, IReadOnlyList<AttendanceRecordDto>>
{
    public async Task<IReadOnlyList<AttendanceRecordDto>> Handle(
        GetAllAttendanceRecordsQuery request, CancellationToken cancellationToken)
    {
        var attendance = await unitOfWork.Repository<AttendanceRecord>().GetAllAsync(cancellationToken);
        return attendance.Select(AttendanceRecordDto.From).ToList();
    }
}
