using System.Text.Json;
using static PaquitoBot.Application.Services.SyncByStudent.FrappeSnapshotJson;

namespace PaquitoBot.Application.Services.SyncByStudent;

internal sealed class FrappeStudentSnapshotPersister(
    FrappeSnapshotStudentWriter students,
    FrappeSnapshotCatalogWriter catalog,
    FrappeSnapshotEnrollmentWriter enrollments,
    FrappeSnapshotScheduleWriter schedules,
    FrappeSnapshotEvaluationWriter evaluations,
    FrappeSnapshotFinanceWriter finances)
{
    public async Task<int> PersistAsync(JsonElement snapshot, Guid userId, string email,
        CancellationToken cancellationToken)
    {
        var studentRow = snapshot.GetProperty("student");
        if (!string.Equals(Required(studentRow, "email_address"), email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Frappe returned a different student's records.");

        var now = DateTime.UtcNow;
        var studentName = Required(studentRow, "name");
        var student = await students.WriteAsync(studentRow, email, now, cancellationToken);
        var (periods, programs, courses, instructors, catalogCount) = await catalog.WriteAsync(
            snapshot, now, cancellationToken);
        var (sections, enrollmentCount) = await enrollments.WriteAsync(
            snapshot, student, studentName, periods, programs, courses, instructors, now, cancellationToken);
        var scheduleCount = await schedules.WriteAsync(snapshot, studentName, sections, instructors, now, cancellationToken);
        var evaluationCount = await evaluations.WriteAsync(snapshot, student, studentName, sections, now, cancellationToken);
        var financeCount = await finances.WriteAsync(snapshot, student, studentName, now, cancellationToken);
        await students.LinkAsync(userId, student, cancellationToken);

        return 1 + catalogCount + enrollmentCount + scheduleCount + evaluationCount + financeCount;
    }
}
