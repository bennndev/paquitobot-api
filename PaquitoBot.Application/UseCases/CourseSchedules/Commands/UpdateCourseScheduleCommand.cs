using PaquitoBot.Domain.Validations;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.CourseSchedules;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSchedules.Commands;

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

internal sealed class UpdateCourseScheduleCommandHandler(
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

        await unitOfWork.Repository<CourseSection>().EnsureExistsAsync("CourseSection", request.SectionId, cancellationToken);

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
