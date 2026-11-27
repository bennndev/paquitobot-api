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

internal sealed class FrappeSnapshotStudentWriter(IUnitOfWork unitOfWork)
{
    public async Task<Student> WriteAsync(JsonElement studentRow, string email, DateTime now, CancellationToken cancellationToken)
    {
        var studentName = Required(studentRow, "name");
        var student = await Upsert(unitOfWork.Repository<Student>(), studentName, entity =>
        {
            var source = new FrappeStudentDto(
                studentName,
                studentName,
                Join(Optional(studentRow, "first_name"), Optional(studentRow, "middle_name")),
                Optional(studentRow, "last_name"),
                Optional(studentRow, "student_name"),
                email,
                Optional(studentRow, "student_mobile_number"),
                Date(studentRow, "date_of_birth"),
                Optional(studentRow, "gender"),
                Optional(studentRow, "enabled") == "1" ? "active" : "inactive",
                null);
            StudentSyncMapper.Apply(source, entity);
            MarkSharedRecord(entity, now);
        }, cancellationToken);

        return student;
    }

    public async Task LinkAsync(Guid userId, Student student, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.UserStudents.PairExistsAsync(userId, student.StudentId,
                cancellationToken))
            await unitOfWork.UserStudents.AddAsync(new UserStudent
            {
                UserId = userId, StudentId = student.StudentId,
                IsPrimary = !await unitOfWork.UserStudents.UserHasPrimaryAsync(userId, cancellationToken: cancellationToken)
            }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
