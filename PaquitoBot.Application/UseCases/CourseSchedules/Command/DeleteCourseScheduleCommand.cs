using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSchedules.Command;

public record DeleteCourseScheduleCommand(Guid ScheduleId) : IRequest;

public class DeleteCourseScheduleCommandValidator
    : AbstractValidator<DeleteCourseScheduleCommand>
{
    public DeleteCourseScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleId).NotEmpty();
    }
}

public class DeleteCourseScheduleCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCourseScheduleCommand>
{
    public async Task Handle(
        DeleteCourseScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var schedule = await unitOfWork.Repository<CourseSchedule>()
            .GetByIdAsync(request.ScheduleId, cancellationToken)
            ?? throw new NotFoundException("CourseSchedule", request.ScheduleId);

        unitOfWork.Repository<CourseSchedule>().Remove(schedule);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
