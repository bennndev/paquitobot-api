using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Courses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Courses.Command;

public record CreateCourseCommand(
    string FrappeExternalId,
    string Name,
    string? Code = null,
    string? Description = null,
    int? Credits = null,
    string? Status = null) : IRequest<CourseDto>;

public class CreateCourseCommandValidator
    : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FrappeExternalId)
            .NotEmpty()
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .MaximumLength(140);

        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .MaximumLength(255);

        RuleFor(x => x.Code).MaximumLength(80);
        RuleFor(x => x.Credits).GreaterThanOrEqualTo(0).When(x => x.Credits.HasValue);
        RuleFor(x => x.Status).MaximumLength(40);
    }
}

public class CreateCourseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCourseCommand, CourseDto>
{
    public async Task<CourseDto> Handle(
        CreateCourseCommand request,
        CancellationToken cancellationToken)
    {
        var course = new Course
        {
            FrappeExternalId = TextNormalizer.Required(
                request.FrappeExternalId),
            Name = TextNormalizer.Required(request.Name),
            Code = TextNormalizer.Optional(request.Code),
            Description = TextNormalizer.Optional(request.Description),
            Credits = request.Credits,
            Status = TextNormalizer.Optional(request.Status)
        };

        await unitOfWork.Repository<Course>()
            .AddAsync(course, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CourseDto.From(course);
    }
}
