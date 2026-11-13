using MediatR;
using PaquitoBot.Application.DTOs.CourseSchedules;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSchedules.Query;

public record GetAllCourseSchedulesQuery : IRequest<IReadOnlyList<CourseScheduleDto>>;

public class GetAllCourseSchedulesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllCourseSchedulesQuery, IReadOnlyList<CourseScheduleDto>>
{
    public async Task<IReadOnlyList<CourseScheduleDto>> Handle(
        GetAllCourseSchedulesQuery request,
        CancellationToken cancellationToken)
    {
        var schedules = await unitOfWork.Repository<CourseSchedule>()
            .GetAllAsync(cancellationToken);

        return schedules.Select(CourseScheduleDto.From).ToList();
    }
}
