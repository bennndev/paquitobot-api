using PaquitoBot.Application.DTOs.Reports;
using PaquitoBot.Application.DTOs.StudentFinances;

namespace PaquitoBot.Application.Services;

public interface IReportExcelExporter
{
    byte[] ExportEnrollments(IReadOnlyList<EnrollmentReportRowDto> rows);
    byte[] ExportAttendance(IReadOnlyList<AttendanceReportRowDto> rows);
    byte[] ExportAssessments(IReadOnlyList<AssessmentReportRowDto> rows);
    byte[] ExportStudentFinance(StudentFinancialStatementDto statement);
}
