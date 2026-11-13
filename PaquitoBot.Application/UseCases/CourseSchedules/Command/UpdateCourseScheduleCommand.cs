using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.CourseSchedules;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSchedules.Command;

public record UpdateCourseScheduleCommand(
    Guid ScheduleId,
    Guid SectionId,
    DateOnly ClassDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid? InstructorId = null,
    string? Location = null,
    string? DeliveryMode = null,
    string? Status = null) : IRequest<CourseScheduleDto>;

public class UpdateCourseScheduleCommandValidator
    : AbstractValidator<UpdateCourseScheduleCommand>
{
    public UpdateCourseScheduleCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ScheduleId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.InstructorId).NotEmpty().When(x => x.InstructorId.HasValue);
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("EndTime must be later than StartTime.");
        RuleFor(x => x.Location).MaximumLength(255);
        RuleFor(x => x.DeliveryMode).MaximumLength(40);
        RuleFor(x => x.Status).MaximumLength(40);
    }
}

public class UpdateCourseScheduleCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCourseScheduleCommand, CourseScheduleDto>
{
    public async Task<CourseScheduleDto> Handle(
        UpdateCourseScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var schedule = await unitOfWork.Repository<CourseSchedule>()
            .GetByIdAsync(request.ScheduleId, cancellationToken)
            ?? throw new NotFoundException("CourseSchedule", request.ScheduleId);

        if (!await unitOfWork.Repository<CourseSection>()
            .ExistsAsync(request.SectionId, cancellationToken))
        {
            throw new NotFoundException("CourseSection", request.SectionId);
        }

        if (request.InstructorId.HasValue
            && !await unitOfWork.Repository<Instructor>()
                .ExistsAsync(request.InstructorId.Value, cancellationToken))
        {
            throw new NotFoundException("Instructor", request.InstructorId.Value);
        }

        schedule.SectionId = request.SectionId;
        schedule.InstructorId = request.InstructorId;
        schedule.ClassDate = request.ClassDate;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Location = TextNormalizer.Optional(request.Location);
        schedule.DeliveryMode = TextNormalizer.Optional(request.DeliveryMode);
        schedule.Status = TextNormalizer.Optional(request.Status);
        schedule.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CourseScheduleDto.From(schedule);
    }
}
