using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.ProgramEnrollments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Command;

public record UpdateProgramEnrollmentUseCaseCommand(
    Guid ProgramEnrollmentId,
    Guid StudentId,
    Guid ProgramId,
    Guid? AcademicPeriodId = null,
    DateOnly? EnrollmentDate = null,
    string? Status = null) : IRequest<ProgramEnrollmentDto>;

public class UpdateProgramEnrollmentUseCaseCommandValidator
    : AbstractValidator<UpdateProgramEnrollmentUseCaseCommand>
{
    public UpdateProgramEnrollmentUseCaseCommandValidator()
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

public class UpdateProgramEnrollmentUseCaseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProgramEnrollmentUseCaseCommand, ProgramEnrollmentDto>
{
    public async Task<ProgramEnrollmentDto> Handle(
        UpdateProgramEnrollmentUseCaseCommand request,
        CancellationToken cancellationToken)
    {
        var enrollment = await unitOfWork.Repository<ProgramEnrollment>()
            .GetByIdAsync(request.ProgramEnrollmentId, cancellationToken)
            ?? throw new NotFoundException("ProgramEnrollment", request.ProgramEnrollmentId);

        if (!await unitOfWork.Repository<Student>()
            .ExistsAsync(request.StudentId, cancellationToken))
        {
            throw new NotFoundException("Student", request.StudentId);
        }

        if (!await unitOfWork.Repository<Program>()
            .ExistsAsync(request.ProgramId, cancellationToken))
        {
            throw new NotFoundException("Program", request.ProgramId);
        }

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
