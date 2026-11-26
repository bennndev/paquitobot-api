using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSchedules.Commands;

public record DeleteCourseScheduleCommand(Guid ScheduleId) : IRequest;

public class DeleteCourseScheduleCommandValidator
    : AbstractValidator<DeleteCourseScheduleCommand>
{
    public DeleteCourseScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleId).NotEmpty();
    }
}

internal sealed class DeleteCourseScheduleCommandHandler(
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
