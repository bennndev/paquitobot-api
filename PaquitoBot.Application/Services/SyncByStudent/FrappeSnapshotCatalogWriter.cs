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

internal sealed class FrappeSnapshotCatalogWriter(IUnitOfWork unitOfWork)
{
    public async Task<(Dictionary<string, AcademicPeriod> Periods, Dictionary<string, Program> Programs,
        Dictionary<string, Course> Courses, Dictionary<string, Instructor> Instructors, int Count)> WriteAsync(
        JsonElement snapshot, DateTime now, CancellationToken cancellationToken)
    {
        var processed = 0;
        var periods = new Dictionary<string, AcademicPeriod>();
        foreach (var row in Rows(snapshot, "terms"))
        {
            var name = Required(row, "name");
            periods[name] = await Upsert(unitOfWork.Repository<AcademicPeriod>(), name, entity =>
            {
                var period = new FrappeAcademicPeriodDto(
                    name,
                    Optional(row, "term_name"),
                    name,
                    Date(row, "term_start_date"),
                    Date(row, "term_end_date"),
                    "active",
                    null);
                AcademicPeriodSyncMapper.Apply(period, entity);
                MarkSharedRecord(entity, now);
            }, cancellationToken);
            processed++;
        }

        var programs = new Dictionary<string, Program>();
        foreach (var row in Rows(snapshot, "programs"))
        {
            var name = Required(row, "name");
            programs[name] = await Upsert(unitOfWork.Repository<Program>(), name, entity =>
            {
                var program = new FrappeProgramDto(
                    name,
                    Optional(row, "program_abbreviation"),
                    Optional(row, "program_name") ?? name,
                    Optional(row, "short_description"),
                    "active",
                    null);
                ProgramSyncMapper.Apply(program, entity);
                MarkSharedRecord(entity, now);
            }, cancellationToken);
            processed++;
        }

        var courses = new Dictionary<string, Course>();
        foreach (var row in Rows(snapshot, "courses"))
        {
            var name = Required(row, "name");
            courses[name] = await Upsert(unitOfWork.Repository<Course>(), name, entity =>
            {
                var credit = Number(row, "course_credit");
                var credits = credit is null ? (int?)null : checked((int)Math.Round(credit.Value));
                var course = new FrappeCourseDto(
                    name,
                    Optional(row, "course_abbreviation"),
                    Optional(row, "course_name") ?? name,
                    Optional(row, "short_description"),
                    credits,
                    "active",
                    null);
                CourseSyncMapper.Apply(course, entity);
                MarkSharedRecord(entity, now);
            }, cancellationToken);
            processed++;
        }

        var instructors = new Dictionary<string, Instructor>();
        foreach (var row in Rows(snapshot, "faculty"))
        {
            var name = Required(row, "name");
            instructors[name] = await Upsert(unitOfWork.Repository<Instructor>(), name, entity =>
            {
                var instructor = new FrappeInstructorDto(
                    name,
                    Optional(row, "first_name"),
                    Optional(row, "last_name"),
                    Optional(row, "faculty_name"),
                    Optional(row, "email_address"),
                    Optional(row, "phone_number"),
                    Optional(row, "status"),
                    null);
                InstructorSyncMapper.Apply(instructor, entity);
                MarkSharedRecord(entity, now);
            }, cancellationToken);
            processed++;
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var row in Rows(snapshot, "courses"))
        {
            var courseName = Required(row, "name");
            var programName = Optional(row, "program");
            if (programName is null || !programs.TryGetValue(programName, out var program))
            {
                continue;
            }
            var course = courses[courseName];
            if (!await unitOfWork.Repository<ProgramCourse>().AnyAsync(x => x.ProgramId == program.ProgramId &&
                    x.CourseId == course.CourseId, cancellationToken))
                await unitOfWork.Repository<ProgramCourse>().AddAsync(new ProgramCourse
                {
                    ProgramId = program.ProgramId, CourseId = course.CourseId, IsRequired = true
                }, cancellationToken);
        }
        return (periods, programs, courses, instructors, processed);
    }
}
