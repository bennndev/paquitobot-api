using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class ProgramEnrollmentConfiguration : IEntityTypeConfiguration<ProgramEnrollment>
{
    public void Configure(EntityTypeBuilder<ProgramEnrollment> entity)
    {
        entity.HasKey(e => e.ProgramEnrollmentId).HasName("program_enrollments_pkey");

        entity.ToTable("program_enrollments", "paquito");

        entity.HasIndex(e => e.AcademicPeriodId, "ix_program_enrollments_period");

        entity.HasIndex(e => e.ProgramId, "ix_program_enrollments_program");

        entity.HasIndex(e => e.StudentId, "ix_program_enrollments_student");

        entity.HasIndex(e => e.FrappeExternalId, "program_enrollments_frappe_external_id_key").IsUnique();

        entity.Property(e => e.ProgramEnrollmentId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("program_enrollment_id");
        entity.Property(e => e.AcademicPeriodId).HasColumnName("academic_period_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.EnrollmentDate).HasColumnName("enrollment_date");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.ProgramId).HasColumnName("program_id");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.StudentId).HasColumnName("student_id");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.AcademicPeriod).WithMany(p => p.ProgramEnrollments)
            .HasForeignKey(d => d.AcademicPeriodId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("program_enrollments_academic_period_id_fkey");

        entity.HasOne(d => d.Program).WithMany(p => p.ProgramEnrollments)
            .HasForeignKey(d => d.ProgramId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("program_enrollments_program_id_fkey");

        entity.HasOne(d => d.Student).WithMany(p => p.ProgramEnrollments)
            .HasForeignKey(d => d.StudentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("program_enrollments_student_id_fkey");
    }
}
