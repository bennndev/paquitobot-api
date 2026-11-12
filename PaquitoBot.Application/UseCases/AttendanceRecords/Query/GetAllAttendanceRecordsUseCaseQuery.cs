using MediatR;
using PaquitoBot.Application.DTOs.AttendanceRecords;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AttendanceRecords.Query;

public record GetAllAttendanceRecordsUseCaseQuery : IRequest<IReadOnlyList<AttendanceRecordDto>>;

internal sealed class GetAllAttendanceRecordsUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAttendanceRecordsUseCaseQuery, IReadOnlyList<AttendanceRecordDto>>
{
    public async Task<IReadOnlyList<AttendanceRecordDto>> Handle(
        GetAllAttendanceRecordsUseCaseQuery request, CancellationToken cancellationToken)
    {
        var attendance = await unitOfWork.Repository<AttendanceRecord>().GetAllAsync(cancellationToken);
        return attendance.Select(AttendanceRecordDto.From).ToList();
    }
}
