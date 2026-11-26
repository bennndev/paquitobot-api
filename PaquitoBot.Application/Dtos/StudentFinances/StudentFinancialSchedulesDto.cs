namespace PaquitoBot.Application.Dtos.StudentFinances;

public sealed record StudentScheduleTuitionFeeDto(
    Guid TuitionFeeId,
    string Description,
    string CurrencyCode,
    decimal OriginalAmount,
    decimal? ReportedOutstandingBalance,
    DateOnly? DueDate,
    string? Status);

public sealed record StudentFeeScheduleDto(
    Guid FeeScheduleId,
    string Name,
    Guid? AcademicPeriodId,
    DateOnly? DueDate,
    decimal? ScheduledAmount,
    string? Status,
    IReadOnlyList<StudentScheduleTuitionFeeDto> StudentTuitionFees);

public sealed record StudentFeeStructureDto(
    Guid FeeStructureId,
    string Name,
    string CurrencyCode,
    decimal? CatalogTotalAmount,
    string? Status,
    IReadOnlyList<StudentFeeScheduleDto> Schedules);

public sealed record StudentEnrollmentSchedulesDto(
    Guid ProgramEnrollmentId,
    Guid ProgramId,
    string ProgramName,
    Guid? AcademicPeriodId,
    string? EnrollmentStatus,
    IReadOnlyList<StudentFeeStructureDto> FeeStructures);

public sealed record StudentFinancialSchedulesDto(
    Guid StudentId,
    Guid? ProgramIdFilter,
    Guid? AcademicPeriodIdFilter,
    IReadOnlyList<StudentEnrollmentSchedulesDto> Enrollments);
