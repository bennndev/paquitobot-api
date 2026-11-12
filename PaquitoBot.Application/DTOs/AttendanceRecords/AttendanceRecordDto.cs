using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.AttendanceRecords;

public record AttendanceRecordDto(
    Guid AttendanceId,
    string? FrappeExternalId,
    Guid SectionId,
    Guid StudentId,
    DateOnly AttendanceDate,
    string Status,
    string? Notes,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static AttendanceRecordDto From(AttendanceRecord attendance)
    {
        return new AttendanceRecordDto(
            attendance.AttendanceId,
            attendance.FrappeExternalId,
            attendance.SectionId,
            attendance.StudentId,
            attendance.AttendanceDate,
            attendance.Status,
            attendance.Notes,
            attendance.FrappeModifiedAt,
            attendance.SyncedAt,
            attendance.CreatedAt,
            attendance.UpdatedAt
        );
    }
}
