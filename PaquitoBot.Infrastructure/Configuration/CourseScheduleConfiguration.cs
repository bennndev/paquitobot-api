using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class CourseScheduleConfiguration : IEntityTypeConfiguration<CourseSchedule>
{
    public void Configure(EntityTypeBuilder<CourseSchedule> entity)
    {
        entity.HasKey(e => e.ScheduleId).HasName("course_schedules_pkey");

        entity.ToTable("course_schedules", "paquito");

        entity.HasIndex(e => e.FrappeExternalId, "course_schedules_frappe_external_id_key").IsUnique();

        entity.HasIndex(e => new { e.InstructorId, e.ClassDate }, "ix_course_schedules_instructor_date");

        entity.HasIndex(e => new { e.SectionId, e.ClassDate }, "ix_course_schedules_section_date");

        entity.Property(e => e.ScheduleId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("schedule_id");
        entity.Property(e => e.ClassDate).HasColumnName("class_date");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.DeliveryMode)
            .HasMaxLength(40)
            .HasColumnName("delivery_mode");
        entity.Property(e => e.EndTime).HasColumnName("end_time");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
        entity.Property(e => e.Location)
            .HasMaxLength(255)
            .HasColumnName("location");
        entity.Property(e => e.SectionId).HasColumnName("section_id");
        entity.Property(e => e.StartTime).HasColumnName("start_time");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Instructor).WithMany(p => p.CourseSchedules)
            .HasForeignKey(d => d.InstructorId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("course_schedules_instructor_id_fkey");

        entity.HasOne(d => d.Section).WithMany(p => p.CourseSchedules)
            .HasForeignKey(d => d.SectionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("course_schedules_section_id_fkey");
    }
}
