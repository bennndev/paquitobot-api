using MediatR;
using PaquitoBot.Application.Dtos.AttendanceRecords;
using PaquitoBot.Domain.Repositories;

namespace PaquitoBot.Application.UseCases.AttendanceRecords.Queries;

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
