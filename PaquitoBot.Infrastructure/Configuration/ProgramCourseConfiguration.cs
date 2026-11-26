using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class ProgramCourseConfiguration : IEntityTypeConfiguration<ProgramCourse>
{
    public void Configure(EntityTypeBuilder<ProgramCourse> entity)
    {
        entity.HasKey(e => e.ProgramCourseId).HasName("program_courses_pkey");

        entity.ToTable("program_courses", "paquito");

        entity.HasIndex(e => e.CourseId, "ix_program_courses_course");

        entity.HasIndex(e => new { e.ProgramId, e.CourseId }, "uq_program_courses").IsUnique();

        entity.Property(e => e.ProgramCourseId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("program_course_id");
        entity.Property(e => e.CourseId).HasColumnName("course_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.CycleNumber).HasColumnName("cycle_number");
        entity.Property(e => e.IsRequired)
            .HasDefaultValue(true)
            .HasColumnName("is_required");
        entity.Property(e => e.ProgramId).HasColumnName("program_id");

        entity.HasOne(d => d.Course).WithMany(p => p.ProgramCourses)
            .HasForeignKey(d => d.CourseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("program_courses_course_id_fkey");

        entity.HasOne(d => d.Program).WithMany(p => p.ProgramCourses)
            .HasForeignKey(d => d.ProgramId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("program_courses_program_id_fkey");
    }
}
