using ClosedXML.Excel;
using PaquitoBot.Application.DTOs.Reports;
using PaquitoBot.Application.DTOs.StudentFinances;
using PaquitoBot.Application.UseCases.Reports.Query;
using PaquitoBot.Infrastructure.Services;
using Xunit;

namespace PaquitoBot.Reports.Tests;

public sealed class ReportQualityTests
{
    private readonly ReportExcelExporter exporter = new();

    [Fact]
    public void EnrollmentValidatorRejectsEmptyProgramId()
    {
        var result = new GetEnrollmentReportQueryValidator()
            .Validate(new GetEnrollmentReportQuery(Guid.Empty, null));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void EnrollmentValidatorRejectsEmptyAcademicPeriodId()
    {
        var result = new GetEnrollmentReportQueryValidator()
            .Validate(new GetEnrollmentReportQuery(null, Guid.Empty));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void AssessmentValidatorRejectsEmptyProgramId()
    {
        var result = new GetAssessmentReportQueryValidator()
            .Validate(new GetAssessmentReportQuery(Guid.Empty, null));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void AttendanceValidatorAcceptsSameDayRange()
    {
        var date = new DateOnly(2026, 11, 26);
        var result = new GetAttendanceReportQueryValidator()
            .Validate(new GetAttendanceReportQuery(null, null, date, date));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void AttendancePercentageCountsLateAsAttended()
    {
        var row = new AttendanceReportRowDto(Guid.NewGuid(), "Course", "A",
            4, 2, 0, 2, 0, 0);
        Assert.Equal(100m, row.AttendancePercent);
    }

    [Fact]
    public void AttendancePercentageIsZeroWhenAllAbsent()
    {
        var row = new AttendanceReportRowDto(Guid.NewGuid(), "Course", "A",
            3, 0, 3, 0, 0, 0);
        Assert.Equal(0m, row.AttendancePercent);
    }

    [Fact]
    public void AttendanceExportStoresPercentageAsNumber()
    {
        var bytes = exporter.ExportAttendance([
            new AttendanceReportRowDto(Guid.NewGuid(), "Course", "A", 3, 1, 1, 1, 0, 0)
        ]);
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var cell = workbook.Worksheet("Attendance").Cell(2, 9);
        Assert.Equal(66.67d, cell.GetDouble(), 2);
        Assert.False(cell.HasFormula);
    }

    [Theory]
    [InlineData("=HYPERLINK(\"http://invalid.example\",\"click\")")]
    [InlineData("+1+2")]
    [InlineData("@SUM(1,2)")]
    [InlineData("-1+2")]
    public void ExportedNamesAreLiteralTextNotFormulas(string unsafeLabel)
    {
        var bytes = exporter.ExportEnrollments([
            new EnrollmentReportRowDto(Guid.NewGuid(), unsafeLabel, null, "2026-II", 1, 1)
        ]);
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var cell = workbook.Worksheet("Enrollments").Cell(2, 1);
        Assert.False(cell.HasFormula);
        Assert.Equal(unsafeLabel, cell.GetString());
    }

    [Fact]
    public void FinancialExportNeverMixesCurrencies()
    {
        var statement = new StudentFinancialStatementDto(
            Guid.NewGuid(), new DateOnly(2026, 11, 26),
            [
                new CurrencyFinancialSummaryDto("PEN", 500m, 200m, 300m, 0m, 0m, null),
                new CurrencyFinancialSummaryDto("USD", 100m, 20m, 80m, 0m, 0m, null)
            ],
            [], 0);
        var bytes = exporter.ExportStudentFinance(statement);
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var sheet = workbook.Worksheet("Financial summary");
        Assert.Equal("PEN", sheet.Cell(2, 1).GetString());
        Assert.Equal("USD", sheet.Cell(3, 1).GetString());
        Assert.Equal(300d, sheet.Cell(2, 4).GetDouble());
        Assert.Equal(80d, sheet.Cell(3, 4).GetDouble());
    }

    [Fact]
    public void EmptyFinancialStatementExportsValidWorkbook()
    {
        var statement = new StudentFinancialStatementDto(
            Guid.NewGuid(), new DateOnly(2026, 11, 26), [], [], 0);
        var bytes = exporter.ExportStudentFinance(statement);
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal(3, workbook.Worksheets.Count);
        Assert.Equal("Currency", workbook.Worksheet("Financial summary").Cell(1, 1).GetString());
        Assert.Equal("Description", workbook.Worksheet("Obligations").Cell(1, 1).GetString());
    }
}
