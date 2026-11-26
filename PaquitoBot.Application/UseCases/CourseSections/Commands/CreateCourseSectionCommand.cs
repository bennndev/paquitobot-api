using PaquitoBot.Domain.Validations;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.CourseSections;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.CourseSections.Commands;

public record CreateCourseSectionCommand(
    Guid CourseId,
    Guid AcademicPeriodId,
    string? FrappeExternalId = null,
    string? SectionCode = null,
    string? InstructorName = null,
    string? DeliveryMode = null,
    int? Capacity = null,
    string? Status = null) : IRequest<CourseSectionDto>;

public class CreateCourseSectionCommandValidator
    : AbstractValidator<CreateCourseSectionCommand>
{
    public CreateCourseSectionCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.AcademicPeriodId).NotEmpty();
        RuleFor(x => x.FrappeExternalId).MaximumLength(140);
        RuleFor(x => x.SectionCode).MaximumLength(80);
        RuleFor(x => x.InstructorName).MaximumLength(160);
        RuleFor(x => x.DeliveryMode).MaximumLength(40);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(0).When(x => x.Capacity.HasValue);
        RuleFor(x => x.Status).MaximumLength(40);
    }
}

internal sealed class CreateCourseSectionCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCourseSectionCommand, CourseSectionDto>
{
    public async Task<CourseSectionDto> Handle(
        CreateCourseSectionCommand request,
        CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<Course>().EnsureExistsAsync("Course", request.CourseId, cancellationToken);

        await unitOfWork.Repository<AcademicPeriod>().EnsureExistsAsync("AcademicPeriod", request.AcademicPeriodId, cancellationToken);

        var section = new CourseSection
        {
            CourseId = request.CourseId,
            AcademicPeriodId = request.AcademicPeriodId,
            FrappeExternalId = TextNormalizer.Optional(request.FrappeExternalId),
            SectionCode = TextNormalizer.Optional(request.SectionCode),
            InstructorName = TextNormalizer.Optional(request.InstructorName),
            DeliveryMode = TextNormalizer.Optional(request.DeliveryMode),
            Capacity = request.Capacity,
            Status = TextNormalizer.Optional(request.Status)
        };

        await unitOfWork.Repository<CourseSection>()
            .AddAsync(section, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CourseSectionDto.From(section);
    }
}
