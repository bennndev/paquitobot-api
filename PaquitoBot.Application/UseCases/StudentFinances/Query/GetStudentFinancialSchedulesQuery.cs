using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.StudentFinances;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.StudentFinances.Query;

public record GetStudentFinancialSchedulesQuery(
    Guid StudentId,
    Guid RequestingUserId,
    Guid? ProgramId = null,
    Guid? AcademicPeriodId = null) : IRequest<StudentFinancialSchedulesDto>;

public sealed class GetStudentFinancialSchedulesQueryValidator
    : AbstractValidator<GetStudentFinancialSchedulesQuery>
{
    public GetStudentFinancialSchedulesQueryValidator()
    {
        RuleFor(q => q.StudentId).NotEmpty();
        RuleFor(q => q.RequestingUserId).NotEmpty();
        RuleFor(q => q.ProgramId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty);
        RuleFor(q => q.AcademicPeriodId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty);
    }
}

public sealed class GetStudentFinancialSchedulesQueryHandler(
    IFinancialReadRepository repository)
    : IRequestHandler<GetStudentFinancialSchedulesQuery, StudentFinancialSchedulesDto>
{
    public async Task<StudentFinancialSchedulesDto> Handle(
        GetStudentFinancialSchedulesQuery request,
        CancellationToken cancellationToken)
    {
        // Do not reveal whether another student's record exists.
        if (!await repository.CanUserAccessStudentAsync(
                request.RequestingUserId, request.StudentId, cancellationToken) ||
            !await repository.StudentExistsAsync(request.StudentId, cancellationToken))
        {
            throw new NotFoundException("Student", request.StudentId);
        }

        var enrollments = await repository.GetProgramEnrollmentsAsync(
            request.StudentId, request.ProgramId, request.AcademicPeriodId, cancellationToken);

        if (enrollments.Count == 0)
        {
            return new StudentFinancialSchedulesDto(
                request.StudentId, request.ProgramId, request.AcademicPeriodId,
                Array.Empty<StudentEnrollmentSchedulesDto>());
        }

        var programIds = enrollments.Select(e => e.ProgramId).Distinct().ToArray();
        var structures = await repository.GetFeeStructuresForProgramsAsync(
            programIds, cancellationToken);
        var structureIds = structures.Select(s => s.FeeStructureId).ToArray();
        var schedules = await repository.GetFeeSchedulesForStructuresAsync(
            structureIds, request.AcademicPeriodId, cancellationToken);
        var studentFees = await repository.GetTuitionFeesAsync(
            request.StudentId, cancellationToken);

        var items = enrollments.Select(enrollment =>
        {
            // A schedule with a specific period must match the enrollment's
            // period. Unscoped schedules can apply to every period.
            var catalog = structures
                .Where(s => s.ProgramId == enrollment.ProgramId)
                .Select(structure => new StudentFeeStructureDto(
                    structure.FeeStructureId,
                    structure.Name,
                    structure.CurrencyCode,
                    structure.TotalAmount,
                    structure.Status,
                    schedules
                        .Where(schedule =>
                            schedule.FeeStructureId == structure.FeeStructureId &&
                            (!schedule.AcademicPeriodId.HasValue ||
                             (enrollment.AcademicPeriodId.HasValue &&
                              schedule.AcademicPeriodId == enrollment.AcademicPeriodId)))
                        .Select(schedule => new StudentFeeScheduleDto(
                            schedule.FeeScheduleId,
                            schedule.Name,
                            schedule.AcademicPeriodId,
                            schedule.DueDate,
                            schedule.ScheduledAmount,
                            schedule.Status,
                            studentFees
                                .Where(fee => fee.FeeScheduleId == schedule.FeeScheduleId)
                                .Select(fee => new StudentScheduleTuitionFeeDto(
                                    fee.TuitionFeeId, fee.Description, fee.CurrencyCode,
                                    fee.OriginalAmount, fee.OutstandingBalance,
                                    fee.DueDate, fee.Status))
                                .ToList()))
                        .ToList()))
                .ToList();

            return new StudentEnrollmentSchedulesDto(
                enrollment.ProgramEnrollmentId,
                enrollment.ProgramId,
                enrollment.Program.Name,
                enrollment.AcademicPeriodId,
                enrollment.Status,
                catalog);
        }).ToList();

        return new StudentFinancialSchedulesDto(
            request.StudentId, request.ProgramId, request.AcademicPeriodId, items);
    }
}
