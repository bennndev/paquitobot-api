using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Courses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Courses.Command;

public record UpdateCourseCommand(
    Guid CourseId,
    string Name,
    string? Code = null,
    string? Description = null,
    int? Credits = null,
    string? Status = null) : IRequest<CourseDto>;

public class UpdateCourseCommandValidator
    : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.CourseId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .MaximumLength(255);

        RuleFor(x => x.Code).MaximumLength(80);
        RuleFor(x => x.Credits).GreaterThanOrEqualTo(0).When(x => x.Credits.HasValue);
        RuleFor(x => x.Status).MaximumLength(40);
    }
}

public class UpdateCourseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCourseCommand, CourseDto>
{
    public async Task<CourseDto> Handle(
        UpdateCourseCommand request,
        CancellationToken cancellationToken)
    {
        var course = await unitOfWork.Repository<Course>()
            .GetByIdAsync(request.CourseId, cancellationToken)
            ?? throw new NotFoundException(
                "Course", request.CourseId);

        course.Name = TextNormalizer.Required(request.Name);
        course.Code = TextNormalizer.Optional(request.Code);
        course.Description = TextNormalizer.Optional(request.Description);
        course.Credits = request.Credits;
        course.Status = TextNormalizer.Optional(request.Status);
        course.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CourseDto.From(course);
    }
}
