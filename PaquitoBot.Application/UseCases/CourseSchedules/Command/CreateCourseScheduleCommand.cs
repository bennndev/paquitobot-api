using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.CourseSchedules;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSchedules.Command;

public record CreateCourseScheduleCommand(
    Guid SectionId,
    DateOnly ClassDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid? InstructorId = null,
    string? FrappeExternalId = null,
    string? Location = null,
    string? DeliveryMode = null,
    string? Status = null) : IRequest<CourseScheduleDto>;

public class CreateCourseScheduleCommandValidator
    : AbstractValidator<CreateCourseScheduleCommand>
{
    public CreateCourseScheduleCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.InstructorId).NotEmpty().When(x => x.InstructorId.HasValue);
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("EndTime must be later than StartTime.");
        RuleFor(x => x.FrappeExternalId).MaximumLength(140);
        RuleFor(x => x.Location).MaximumLength(255);
        RuleFor(x => x.DeliveryMode).MaximumLength(40);
        RuleFor(x => x.Status).MaximumLength(40);
    }
}

public class CreateCourseScheduleCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCourseScheduleCommand, CourseScheduleDto>
{
    public async Task<CourseScheduleDto> Handle(
        CreateCourseScheduleCommand request,
        CancellationToken cancellationToken)
    {
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

        var schedule = new CourseSchedule
        {
            SectionId = request.SectionId,
            InstructorId = request.InstructorId,
            ClassDate = request.ClassDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            Location = TextNormalizer.Optional(request.Location),
            DeliveryMode = TextNormalizer.Optional(request.DeliveryMode),
            Status = TextNormalizer.Optional(request.Status)
        };

        await unitOfWork.Repository<CourseSchedule>()
            .AddAsync(schedule, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CourseScheduleDto.From(schedule);
    }
}
