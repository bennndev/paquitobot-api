using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.CourseSections;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSections.Command;

public record UpdateCourseSectionCommand(
    Guid SectionId,
    Guid CourseId,
    Guid AcademicPeriodId,
    string? SectionCode = null,
    string? InstructorName = null,
    string? DeliveryMode = null,
    int? Capacity = null,
    string? Status = null) : IRequest<CourseSectionDto>;

public class UpdateCourseSectionCommandValidator
    : AbstractValidator<UpdateCourseSectionCommand>
{
    public UpdateCourseSectionCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.AcademicPeriodId).NotEmpty();
        RuleFor(x => x.SectionCode).MaximumLength(80);
        RuleFor(x => x.InstructorName).MaximumLength(160);
        RuleFor(x => x.DeliveryMode).MaximumLength(40);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(0).When(x => x.Capacity.HasValue);
        RuleFor(x => x.Status).MaximumLength(40);
    }
}

public class UpdateCourseSectionCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCourseSectionCommand, CourseSectionDto>
{
    public async Task<CourseSectionDto> Handle(
        UpdateCourseSectionCommand request,
        CancellationToken cancellationToken)
    {
        var section = await unitOfWork.Repository<CourseSection>()
            .GetByIdAsync(request.SectionId, cancellationToken)
            ?? throw new NotFoundException("CourseSection", request.SectionId);

        if (!await unitOfWork.Repository<Course>()
            .ExistsAsync(request.CourseId, cancellationToken))
        {
            throw new NotFoundException("Course", request.CourseId);
        }

        if (!await unitOfWork.Repository<AcademicPeriod>()
            .ExistsAsync(request.AcademicPeriodId, cancellationToken))
        {
            throw new NotFoundException("AcademicPeriod", request.AcademicPeriodId);
        }

        section.CourseId = request.CourseId;
        section.AcademicPeriodId = request.AcademicPeriodId;
        section.SectionCode = TextNormalizer.Optional(request.SectionCode);
        section.InstructorName = TextNormalizer.Optional(request.InstructorName);
        section.DeliveryMode = TextNormalizer.Optional(request.DeliveryMode);
        section.Capacity = request.Capacity;
        section.Status = TextNormalizer.Optional(request.Status);
        section.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CourseSectionDto.From(section);
    }
}
