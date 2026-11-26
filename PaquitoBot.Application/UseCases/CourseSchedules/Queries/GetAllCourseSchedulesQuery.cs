using MediatR;
using PaquitoBot.Application.Dtos.CourseSchedules;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSchedules.Queries;

public record GetAllCourseSchedulesQuery : IRequest<IReadOnlyList<CourseScheduleDto>>;

internal sealed class GetAllCourseSchedulesQueryHandler(IUnitOfWork unitOfWork)
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
