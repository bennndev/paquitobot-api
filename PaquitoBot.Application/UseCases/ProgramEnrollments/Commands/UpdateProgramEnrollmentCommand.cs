using PaquitoBot.Domain.Validations;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.ProgramEnrollments;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Commands;

public record UpdateProgramEnrollmentCommand(
    Guid ProgramEnrollmentId,
    Guid StudentId,
    Guid ProgramId,
    Guid? AcademicPeriodId = null,
    DateOnly? EnrollmentDate = null,
    string? Status = null) : IRequest<ProgramEnrollmentDto>;

public class UpdateProgramEnrollmentCommandValidator
    : AbstractValidator<UpdateProgramEnrollmentCommand>
{
    public UpdateProgramEnrollmentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ProgramEnrollmentId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.ProgramId).NotEmpty();
        RuleFor(x => x.AcademicPeriodId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("AcademicPeriodId must be a non-empty GUID when provided.");
        RuleFor(x => x.Status).MaximumLength(40);
    }
}

internal sealed class UpdateProgramEnrollmentCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProgramEnrollmentCommand, ProgramEnrollmentDto>
{
    public async Task<ProgramEnrollmentDto> Handle(
        UpdateProgramEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        var enrollment = await unitOfWork.Repository<ProgramEnrollment>()
            .GetByIdAsync(request.ProgramEnrollmentId, cancellationToken)
            ?? throw new NotFoundException("ProgramEnrollment", request.ProgramEnrollmentId);

        await unitOfWork.Repository<Student>().EnsureExistsAsync("Student", request.StudentId, cancellationToken);

        await unitOfWork.Repository<Program>().EnsureExistsAsync("Program", request.ProgramId, cancellationToken);

        if (request.AcademicPeriodId is Guid periodId &&
            !await unitOfWork.Repository<AcademicPeriod>()
                .ExistsAsync(periodId, cancellationToken))
        {
            throw new NotFoundException("AcademicPeriod", periodId);
        }

        enrollment.StudentId = request.StudentId;
        enrollment.ProgramId = request.ProgramId;
        enrollment.AcademicPeriodId = request.AcademicPeriodId;
        enrollment.EnrollmentDate = request.EnrollmentDate;
        enrollment.Status = TextNormalizer.Optional(request.Status);
        enrollment.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProgramEnrollmentDto.From(enrollment);
    }
}
