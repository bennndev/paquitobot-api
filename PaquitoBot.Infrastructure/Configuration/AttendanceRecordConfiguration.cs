using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> entity)
    {
        entity.HasKey(e => e.AttendanceId).HasName("attendance_records_pkey");

        entity.ToTable("attendance_records", "paquito");

        entity.HasIndex(e => e.FrappeExternalId, "attendance_records_frappe_external_id_key").IsUnique();

        entity.HasIndex(e => e.SectionId, "ix_attendance_section");

        entity.HasIndex(e => new { e.StudentId, e.AttendanceDate }, "ix_attendance_student_date");

        entity.Property(e => e.AttendanceId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("attendance_id");
        entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.Notes).HasColumnName("notes");
        entity.Property(e => e.SectionId).HasColumnName("section_id");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.StudentId).HasColumnName("student_id");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Section).WithMany(p => p.AttendanceRecords)
            .HasForeignKey(d => d.SectionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("attendance_records_section_id_fkey");

        entity.HasOne(d => d.Student).WithMany(p => p.AttendanceRecords)
            .HasForeignKey(d => d.StudentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("attendance_records_student_id_fkey");
    }
}
