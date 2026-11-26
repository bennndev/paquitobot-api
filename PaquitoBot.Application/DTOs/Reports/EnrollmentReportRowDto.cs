namespace PaquitoBot.Application.DTOs.Reports;

public sealed record EnrollmentReportRowDto(
    Guid ProgramId,
    string ProgramName,
    Guid? AcademicPeriodId,
    string AcademicPeriodName,
    int EnrollmentRecords,
    int DistinctStudents);
