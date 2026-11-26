using PaquitoBot.Domain.Validations;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.ProgramCourses;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramCourses.Commands;

public record CreateProgramCourseCommand(
    Guid ProgramId,
    Guid CourseId,
    int? CycleNumber = null,
    bool IsRequired = true) : IRequest<ProgramCourseDto>;

public class CreateProgramCourseCommandValidator
    : AbstractValidator<CreateProgramCourseCommand>
{
    public CreateProgramCourseCommandValidator()
    {
        RuleFor(x => x.ProgramId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.CycleNumber)
            .Must(x => !x.HasValue || x.Value > 0)
            .WithMessage("CycleNumber must be greater than zero when provided.");
    }
}

internal sealed class CreateProgramCourseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProgramCourseCommand, ProgramCourseDto>
{
    public async Task<ProgramCourseDto> Handle(
        CreateProgramCourseCommand request,
        CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<Program>().EnsureExistsAsync("Program", request.ProgramId, cancellationToken);

        await unitOfWork.Repository<Course>().EnsureExistsAsync("Course", request.CourseId, cancellationToken);

        var link = new ProgramCourse
        {
            ProgramId = request.ProgramId,
            CourseId = request.CourseId,
            CycleNumber = request.CycleNumber,
            IsRequired = request.IsRequired
        };

        await unitOfWork.Repository<ProgramCourse>()
            .AddAsync(link, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProgramCourseDto.From(link);
    }
}
