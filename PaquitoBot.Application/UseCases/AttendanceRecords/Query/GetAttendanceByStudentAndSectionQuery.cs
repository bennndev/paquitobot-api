using MediatR;
using PaquitoBot.Application.DTOs.AttendanceRecords;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.AttendanceRecords.Query;

public record GetAttendanceByStudentAndSectionQuery(Guid StudentId, Guid SectionId) : IRequest<IReadOnlyList<AttendanceRecordDto>>;

internal sealed class GetAttendanceByStudentAndSectionQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAttendanceByStudentAndSectionQuery, IReadOnlyList<AttendanceRecordDto>>
{
    public async Task<IReadOnlyList<AttendanceRecordDto>> Handle(GetAttendanceByStudentAndSectionQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.AttendanceRecords.GetByStudentAndSectionAsync(request.StudentId, request.SectionId, cancellationToken);
        return items.Select(AttendanceRecordDto.From).ToList();
    }
}
