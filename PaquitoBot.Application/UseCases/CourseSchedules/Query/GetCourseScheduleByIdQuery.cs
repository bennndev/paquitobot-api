using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.CourseSchedules;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSchedules.Query;

public record GetCourseScheduleByIdQuery(Guid ScheduleId) : IRequest<CourseScheduleDto>;

public class GetCourseScheduleByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCourseScheduleByIdQuery, CourseScheduleDto>
{
    public async Task<CourseScheduleDto> Handle(
        GetCourseScheduleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var schedule = await unitOfWork.Repository<CourseSchedule>()
            .GetByIdAsync(request.ScheduleId, cancellationToken)
            ?? throw new NotFoundException("CourseSchedule", request.ScheduleId);

        return CourseScheduleDto.From(schedule);
    }
}
