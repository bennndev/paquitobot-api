using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class CourseSectionConfiguration : IEntityTypeConfiguration<CourseSection>
{
    public void Configure(EntityTypeBuilder<CourseSection> entity)
    {
        entity.HasKey(e => e.SectionId).HasName("course_sections_pkey");

        entity.ToTable("course_sections", "paquito");

        entity.HasIndex(e => e.FrappeExternalId, "course_sections_frappe_external_id_key").IsUnique();

        entity.HasIndex(e => new { e.AcademicPeriodId, e.CourseId }, "ix_course_sections_period");

        entity.Property(e => e.SectionId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("section_id");
        entity.Property(e => e.AcademicPeriodId).HasColumnName("academic_period_id");
        entity.Property(e => e.Capacity).HasColumnName("capacity");
        entity.Property(e => e.CourseId).HasColumnName("course_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.DeliveryMode)
            .HasMaxLength(40)
            .HasColumnName("delivery_mode");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.InstructorName)
            .HasMaxLength(160)
            .HasColumnName("instructor_name");
        entity.Property(e => e.SectionCode)
            .HasMaxLength(80)
            .HasColumnName("section_code");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.AcademicPeriod).WithMany(p => p.CourseSections)
            .HasForeignKey(d => d.AcademicPeriodId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("course_sections_academic_period_id_fkey");

        entity.HasOne(d => d.Course).WithMany(p => p.CourseSections)
            .HasForeignKey(d => d.CourseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("course_sections_course_id_fkey");
    }
}
