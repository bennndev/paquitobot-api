using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.CourseSchedules;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSchedules.Queries;

public record GetCourseScheduleByIdQuery(Guid ScheduleId) : IRequest<CourseScheduleDto>;

internal sealed class GetCourseScheduleByIdQueryHandler(IUnitOfWork unitOfWork)
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
