using ClosedXML.Excel;
using PaquitoBot.Application.DTOs.Reports;
using PaquitoBot.Application.DTOs.StudentFinances;
using PaquitoBot.Application.UseCases.Reports.Query;
using PaquitoBot.Infrastructure.Services;
using Xunit;

namespace PaquitoBot.Reports.Tests;

public sealed class ReportTests
{
    private readonly ReportExcelExporter exporter = new();

    [Fact]
    public void EnrollmentReportExportsTypedCounts()
    {
        var bytes = exporter.ExportEnrollments([
            new EnrollmentReportRowDto(Guid.NewGuid(), "Software", Guid.NewGuid(), "2026-II", 12, 10)
        ]);
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var sheet = workbook.Worksheet("Enrollments");
        Assert.Equal("Program", sheet.Cell(1, 1).GetString());
        Assert.Equal("Software", sheet.Cell(2, 1).GetString());
        Assert.Equal(12d, sheet.Cell(2, 3).GetDouble());
        Assert.Equal(10d, sheet.Cell(2, 4).GetDouble());
    }

    [Fact]
    public void AttendanceRateExcludesExcusedAndUnknown()
    {
        var row = new AttendanceReportRowDto(Guid.NewGuid(), "Math", "A", 8, 3, 1, 2, 1, 1);
        Assert.Equal(83.33m, row.AttendancePercent);
    }

    [Fact]
    public void AttendanceEmptyRateIsNull()
    {
        var row = new AttendanceReportRowDto(Guid.NewGuid(), "Math", "A", 2, 0, 0, 0, 1, 1);
        Assert.Null(row.AttendancePercent);
    }

    [Fact]
    public void AttendanceDatesMustBeOrdered()
    {
        var validator = new GetAttendanceReportQueryValidator();
        var result = validator.Validate(new GetAttendanceReportQuery(null, null,
            new DateOnly(2026, 11, 25), new DateOnly(2026, 11, 20)));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void AssessmentExportKeepsNormalizedPercentage()
    {
        var bytes = exporter.ExportAssessments([
            new AssessmentReportRowDto(Guid.NewGuid(), "Software", "A", 3, 78.5m)
        ]);
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal(78.5d, workbook.Worksheet("Assessments").Cell(2, 4).GetDouble());
    }

    [Fact]
    public void FinancialExportUsesExistingStatementNumbersAndPreservesCurrencies()
    {
        var studentId = Guid.NewGuid();
        var statement = new StudentFinancialStatementDto(
            studentId, new DateOnly(2026, 11, 26),
            [new CurrencyFinancialSummaryDto("PEN", 500, 200, 300, 50, 0, new DateOnly(2026, 12, 5))],
            [new TuitionFeeBalanceDto(Guid.NewGuid(), null, "Tuition", "PEN",
                500, 200, 300, 0, 300, false, new DateOnly(2026, 12, 5), "Partial", false)], 0);
        var bytes = exporter.ExportStudentFinance(statement);
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal(3, workbook.Worksheets.Count);
        Assert.Equal(300d, workbook.Worksheet("Financial summary").Cell(2, 4).GetDouble());
        Assert.Equal(200d, workbook.Worksheet("Obligations").Cell(2, 4).GetDouble());
        Assert.Equal(studentId.ToString(), workbook.Worksheet("Metadata").Cell(2, 2).GetString());
    }

    [Fact]
    public void EmptyDataExportsHeadersOnly()
    {
        var bytes = exporter.ExportEnrollments([]);
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal("Program", workbook.Worksheet("Enrollments").Cell(1, 1).GetString());
    }

    [Fact]
    public void TextValuesAreNeverExcelFormulas()
    {
        var bytes = exporter.ExportEnrollments([
            new EnrollmentReportRowDto(Guid.NewGuid(), "=2+2", null, "Q", 1, 1)
        ]);
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var cell = workbook.Worksheet("Enrollments").Cell(2, 1);
        Assert.Equal("=2+2", cell.GetString());
        Assert.False(cell.HasFormula);
    }
}
