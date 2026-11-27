using System.Globalization;
using System.Text.Json;
using PaquitoBot.Application.Common.Frappe;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Application.UseCases.Sync.Mapping;

using static PaquitoBot.Application.Services.SyncByStudent.FrappeSnapshotJson;
using static PaquitoBot.Application.Services.SyncByStudent.FrappeSnapshotWrites;

namespace PaquitoBot.Application.Services.SyncByStudent;

internal sealed class FrappeSnapshotScheduleWriter(IUnitOfWork unitOfWork)
{
    public async Task<int> WriteAsync(JsonElement snapshot, string studentName,
        IReadOnlyDictionary<string, CourseSection> sections, IReadOnlyDictionary<string, Instructor> instructors,
        DateTime now, CancellationToken cancellationToken)
    {
        var processed = 0;
        foreach (var row in Rows(snapshot, "schedules"))
        {
            EnsureStudent(row, studentName);
            var name = Required(row, "name");
            var section = sections[Required(row, "student_batch")];
            var facultyName = Optional(row, "faculty");
            section.InstructorName = Optional(row, "faculty_name");
            await Upsert(unitOfWork.Repository<CourseSchedule>(), name, entity =>
            {
                var source = new FrappeCourseScheduleDto(
                    name,
                    Required(row, "student_batch"),
                    facultyName,
                    Date(row, "schedule_date") ?? throw new InvalidOperationException("Schedule date is missing."),
                    Time(row, "from_time"),
                    Time(row, "to_time"),
                    Optional(row, "room"),
                    null,
                    "scheduled",
                    null);
                var instructorId = facultyName is not null && instructors.TryGetValue(facultyName, out var instructor)
                    ? instructor.InstructorId : (Guid?)null;
                CourseScheduleSyncMapper.Apply(source, entity, section.SectionId, instructorId);
                MarkSharedRecord(entity, now);
            }, cancellationToken);
            processed++;
        }
        return processed;
    }
}
