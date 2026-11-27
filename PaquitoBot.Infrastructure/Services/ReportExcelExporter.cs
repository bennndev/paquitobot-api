using ClosedXML.Excel;
using PaquitoBot.Application.DTOs.Reports;
using PaquitoBot.Application.DTOs.StudentFinances;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services;

// XLSX formatting and serialization only; no finance or academic domain calculations.
public sealed class ReportExcelExporter : IReportExcelExporter
{
    public byte[] ExportEnrollments(IReadOnlyList<EnrollmentReportRowDto> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = NewSheet(workbook, "Enrollments", [
            "Program", "Academic period", "Enrollment records", "Distinct students"]);
        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var line = index + 2;
            Text(sheet, line, 1, row.ProgramName);
            Text(sheet, line, 2, row.AcademicPeriodName);
            sheet.Cell(line, 3).Value = row.EnrollmentRecords;
            sheet.Cell(line, 4).Value = row.DistinctStudents;
        }
        Finish(sheet, 4);
        return Save(workbook);
    }

    public byte[] ExportAttendance(IReadOnlyList<AttendanceReportRowDto> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = NewSheet(workbook, "Attendance", [
            "Course", "Section", "Records", "Present", "Absent", "Late", "Excused", "Other", "Attendance %"]);
        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var line = index + 2;
            Text(sheet, line, 1, row.CourseName);
            Text(sheet, line, 2, row.SectionCode);
            sheet.Cell(line, 3).Value = row.Records;
            sheet.Cell(line, 4).Value = row.Present;
            sheet.Cell(line, 5).Value = row.Absent;
            sheet.Cell(line, 6).Value = row.Late;
            sheet.Cell(line, 7).Value = row.Excused;
            sheet.Cell(line, 8).Value = row.Other;
            if (row.AttendancePercent.HasValue)
                sheet.Cell(line, 9).Value = row.AttendancePercent.Value;
        }
        if (rows.Count > 0)
            sheet.Range(2, 9, rows.Count + 1, 9).Style.NumberFormat.Format = "0.00";
        Finish(sheet, 9);
        return Save(workbook);
    }

    public byte[] ExportAssessments(IReadOnlyList<AssessmentReportRowDto> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = NewSheet(workbook, "Assessments", [
            "Course", "Section", "Graded results", "Average score %"]);
        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var line = index + 2;
            Text(sheet, line, 1, row.CourseName);
            Text(sheet, line, 2, row.SectionCode);
            sheet.Cell(line, 3).Value = row.GradedResults;
            sheet.Cell(line, 4).Value = row.AverageScorePercent;
        }
        if (rows.Count > 0)
            sheet.Range(2, 4, rows.Count + 1, 4).Style.NumberFormat.Format = "0.00";
        Finish(sheet, 4);
        return Save(workbook);
    }

    public byte[] ExportStudentFinance(StudentFinancialStatementDto statement)
    {
        using var workbook = new XLWorkbook();
        var summary = NewSheet(workbook, "Financial summary", [
            "Currency", "Scheduled", "Paid", "Outstanding", "Overdue", "Excess", "Next due date"]);
        for (var index = 0; index < statement.Currencies.Count; index++)
        {
            var row = statement.Currencies[index];
            var line = index + 2;
            Text(summary, line, 1, row.CurrencyCode);
            summary.Cell(line, 2).Value = row.TotalScheduled;
            summary.Cell(line, 3).Value = row.TotalPaid;
            summary.Cell(line, 4).Value = row.TotalOutstanding;
            summary.Cell(line, 5).Value = row.TotalOverdue;
            summary.Cell(line, 6).Value = row.ExcessPaymentAmount;
            Text(summary, line, 7, row.NextDueDate?.ToString("yyyy-MM-dd") ?? "");
        }
        if (statement.Currencies.Count > 0)
            summary.Range(2, 2, statement.Currencies.Count + 1, 6).Style.NumberFormat.Format = "#,##0.00";
        Finish(summary, 7);

        var obligations = NewSheet(workbook, "Obligations", [
            "Description", "Currency", "Original", "Confirmed paid", "Outstanding",
            "Excess", "Reported balance", "Due date", "Status", "Reconciliation needed"]);
        for (var index = 0; index < statement.Obligations.Count; index++)
        {
            var row = statement.Obligations[index];
            var line = index + 2;
            Text(obligations, line, 1, row.Description);
            Text(obligations, line, 2, row.CurrencyCode);
            obligations.Cell(line, 3).Value = row.OriginalAmount;
            obligations.Cell(line, 4).Value = row.ConfirmedPaidAmount;
            obligations.Cell(line, 5).Value = row.OutstandingAmount;
            obligations.Cell(line, 6).Value = row.ExcessPaymentAmount;
            if (row.ReportedOutstandingBalance.HasValue)
                obligations.Cell(line, 7).Value = row.ReportedOutstandingBalance.Value;
            Text(obligations, line, 8, row.DueDate?.ToString("yyyy-MM-dd") ?? "");
            Text(obligations, line, 9, row.CalculatedStatus);
            Text(obligations, line, 10, row.ReconciliationRequired ? "Yes" : "No");
        }
        if (statement.Obligations.Count > 0)
            obligations.Range(2, 3, statement.Obligations.Count + 1, 7).Style.NumberFormat.Format = "#,##0.00";
        Finish(obligations, 10);

        var metadata = NewSheet(workbook, "Metadata", ["Key", "Value"]);
        Text(metadata, 2, 1, "Student ID");
        Text(metadata, 2, 2, statement.StudentId.ToString());
        Text(metadata, 3, 1, "Statement as of");
        Text(metadata, 3, 2, statement.AsOf.ToString("yyyy-MM-dd"));
        Text(metadata, 4, 1, "Payments not counted");
        metadata.Cell(4, 2).Value = statement.PaymentsNotCounted;
        Text(metadata, 5, 1, "Source");
        Text(metadata, 5, 2, "Local PaquitoBot statement (Issue #41)");
        Finish(metadata, 2);
        return Save(workbook);
    }

    private static IXLWorksheet NewSheet(XLWorkbook workbook, string name, IReadOnlyList<string> headings)
    {
        var worksheet = workbook.Worksheets.Add(name);
        for (var col = 0; col < headings.Count; col++)
            Text(worksheet, 1, col + 1, headings[col]);
        var header = worksheet.Range(1, 1, 1, headings.Count);
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4167");
        worksheet.SheetView.FreezeRows(1);
        return worksheet;
    }

    private static void Text(IXLWorksheet sheet, int row, int column, string value)
    {
        // Treat external names/labels strictly as values, never as Excel formulas.
        var cell = sheet.Cell(row, column);
        cell.SetValue(value);
        cell.Style.NumberFormat.Format = "@";
    }

    private static void Finish(IXLWorksheet worksheet, int columns)
    {
        var range = worksheet.RangeUsed();
        if (range is not null && range.RowCount() > 1)
            range.SetAutoFilter();
        worksheet.Columns(1, columns).AdjustToContents();
        foreach (var column in worksheet.Columns(1, columns))
            if (column.Width > 55) column.Width = 55;
    }

    private static byte[] Save(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
