using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class UserStudentConfiguration : IEntityTypeConfiguration<UserStudent>
{
    public void Configure(EntityTypeBuilder<UserStudent> entity)
    {
        entity.HasKey(e => e.LinkId).HasName("user_students_pkey");

        entity.ToTable("user_students", "paquito");

        entity.HasIndex(e => e.StudentId, "ix_user_students_student");

        entity.HasIndex(e => new { e.UserId, e.StudentId }, "uq_user_students_pair").IsUnique();

        entity.HasIndex(e => e.UserId, "ux_user_students_primary")
            .IsUnique()
            .HasFilter("(is_primary = true)");

        entity.Property(e => e.LinkId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("link_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.IsPrimary)
            .HasDefaultValue(true)
            .HasColumnName("is_primary");
        entity.Property(e => e.StudentId).HasColumnName("student_id");
        entity.Property(e => e.UserId).HasColumnName("user_id");

        entity.HasOne(d => d.Student).WithMany(p => p.UserStudents)
            .HasForeignKey(d => d.StudentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("user_students_student_id_fkey");

        entity.HasOne(d => d.User).WithOne(p => p.UserStudent)
            .HasForeignKey<UserStudent>(d => d.UserId)
            .HasConstraintName("user_students_user_id_fkey");
    }
}
