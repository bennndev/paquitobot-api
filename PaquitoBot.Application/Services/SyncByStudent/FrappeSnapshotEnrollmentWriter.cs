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

internal sealed class FrappeSnapshotEnrollmentWriter(IUnitOfWork unitOfWork)
{
    public async Task<(Dictionary<string, CourseSection> Sections, int Count)> WriteAsync(
        JsonElement snapshot, Student student, string studentName, IReadOnlyDictionary<string, AcademicPeriod> periods,
        IReadOnlyDictionary<string, Program> programs, IReadOnlyDictionary<string, Course> courses,
        IReadOnlyDictionary<string, Instructor> instructors, DateTime now, CancellationToken cancellationToken)
    {
        var processed = 0;
        var enrollmentRows = Rows(snapshot, "enrollments")
            .Where(row => Optional(row, "docstatus") != "2")
            .ToList();
        foreach (var enrollment in enrollmentRows)
        {
            EnsureStudent(enrollment, studentName);
        }

        var termsByBatch = new Dictionary<string, HashSet<string>>();
        foreach (var enrollment in enrollmentRows)
        {
            var batchName = Optional(enrollment, "student_batch");
            var termName = Optional(enrollment, "academic_term");
            if (batchName is not null && termName is not null)
            {
                if (!termsByBatch.TryGetValue(batchName, out var terms))
                {
                    terms = [];
                    termsByBatch.Add(batchName, terms);
                }

                terms.Add(termName);
            }
        }
        var sections = new Dictionary<string, CourseSection>();
        foreach (var row in Rows(snapshot, "batches"))
        {
            EnsureStudent(row, studentName);
            var name = Required(row, "name");
            var course = courses[Required(row, "course")];
            var termName = termsByBatch.TryGetValue(name, out var terms) && terms.Count == 1
                ? terms.Single()
                : null;
            var startDate = Date(row, "start_date");
            var period = AcademicPeriodResolver.Resolve(termName, startDate, periods, periods.Values);
            sections[name] = await Upsert(unitOfWork.Repository<CourseSection>(), name, entity =>
            {
                var source = new FrappeCourseSectionDto(
                    name,
                    Required(row, "course"),
                    startDate,
                    Date(row, "end_date"),
                    Optional(row, "batch_name") ?? name,
                    null,
                    null,
                    Integer(row, "max_strength"),
                    Optional(row, "disabled") == "1" ? "inactive" : "active",
                    null,
                    termName);
                CourseSectionSyncMapper.Apply(source, entity, course.CourseId, period.AcademicPeriodId);
                MarkSharedRecord(entity, now);
            }, cancellationToken);
            processed++;
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var latestEnrollments = enrollmentRows
            .GroupBy(row => Required(row, "student_batch"))
            .Select(group => group
                .OrderByDescending(row => FrappeTimestampOrder.ParseOrMinimum(Optional(row, "modified")))
                .ThenByDescending(row => Date(row, "enrollment_date"))
                .ThenByDescending(row => Required(row, "name"), StringComparer.Ordinal)
                .First())
            .ToList();
        var seenProgramEnrollments = new HashSet<(Guid ProgramId, Guid PeriodId)>();
        foreach (var row in latestEnrollments)
        {
            var name = Required(row, "name");
            var sectionName = Required(row, "student_batch");
            var section = sections[sectionName];
            var source = new FrappeSectionEnrollmentDto(
                name,
                studentName,
                sectionName,
                Date(row, "enrollment_date"),
                Optional(row, "status"),
                null);
            void ApplyEnrollment(SectionEnrollment entity)
            {
                SectionEnrollmentSyncMapper.Apply(source, entity, student.StudentId, section.SectionId);
                MarkSharedRecord(entity, now);
            }

            var repository = unitOfWork.Repository<SectionEnrollment>();
            var existing = await repository.FirstOrDefaultAsync(entity =>
                entity.StudentId == student.StudentId && entity.SectionId == section.SectionId,
                cancellationToken);
            if (existing is null)
            {
                await Upsert(repository, name, ApplyEnrollment, cancellationToken);
            }
            else
            {
                existing.FrappeExternalId = name;
                ApplyEnrollment(existing);
            }
            var programName = Optional(row, "program");
            var termName = Optional(row, "academic_term");
            if (programName is not null && termName is not null &&
                programs.TryGetValue(programName, out var program) && periods.TryGetValue(termName, out var period) &&
                seenProgramEnrollments.Add((program.ProgramId, period.AcademicPeriodId)))
            {
                var programEnrollment = await unitOfWork.Repository<ProgramEnrollment>().FirstOrDefaultAsync(x =>
                    x.StudentId == student.StudentId && x.ProgramId == program.ProgramId &&
                    x.AcademicPeriodId == period.AcademicPeriodId, cancellationToken);
                if (programEnrollment is null)
                {
                    await unitOfWork.Repository<ProgramEnrollment>().AddAsync(new ProgramEnrollment
                    {
                        StudentId = student.StudentId, ProgramId = program.ProgramId,
                        AcademicPeriodId = period.AcademicPeriodId,
                        EnrollmentDate = Date(row, "enrollment_date"), Status = "active", SyncedAt = now
                    }, cancellationToken);
                }
                else if (programEnrollment.FrappeExternalId is null)
                {
                    programEnrollment.EnrollmentDate = Date(row, "enrollment_date") ?? programEnrollment.EnrollmentDate;
                    programEnrollment.Status = "active";
                    programEnrollment.SyncedAt = now;
                }
            }
            processed++;
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return (sections, processed);
    }
}
