using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class SectionEnrollmentConfiguration : IEntityTypeConfiguration<SectionEnrollment>
{
    public void Configure(EntityTypeBuilder<SectionEnrollment> entity)
    {
        entity.HasKey(e => e.SectionEnrollmentId).HasName("section_enrollments_pkey");

        entity.ToTable("section_enrollments", "paquito");

        entity.HasIndex(e => e.SectionId, "ix_section_enrollments_section");

        entity.HasIndex(e => e.FrappeExternalId, "section_enrollments_frappe_external_id_key").IsUnique();

        entity.HasIndex(e => new { e.StudentId, e.SectionId }, "uq_section_enrollments_pair").IsUnique();

        entity.Property(e => e.SectionEnrollmentId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("section_enrollment_id");
        entity.Property(e => e.AttendancePercentage)
            .HasPrecision(7, 2)
            .HasColumnName("attendance_percentage");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.EnrollmentDate).HasColumnName("enrollment_date");
        entity.Property(e => e.FinalGrade)
            .HasPrecision(10, 2)
            .HasColumnName("final_grade");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.SectionId).HasColumnName("section_id");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.StudentId).HasColumnName("student_id");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Section).WithMany(p => p.SectionEnrollments)
            .HasForeignKey(d => d.SectionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("section_enrollments_section_id_fkey");

        entity.HasOne(d => d.Student).WithMany(p => p.SectionEnrollments)
            .HasForeignKey(d => d.StudentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("section_enrollments_student_id_fkey");
    }
}
