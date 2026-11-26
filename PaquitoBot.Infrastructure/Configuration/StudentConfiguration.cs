using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> entity)
    {
        entity.HasKey(e => e.StudentId).HasName("students_pkey");

        entity.ToTable("students", "paquito");

        entity.HasIndex(e => e.FrappeModifiedAt, "ix_students_sync");

        entity.HasIndex(e => e.FrappeExternalId, "students_frappe_external_id_key").IsUnique();

        entity.Property(e => e.StudentId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("student_id");
        entity.Property(e => e.BirthDate).HasColumnName("birth_date");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Email)
            .HasMaxLength(254)
            .HasColumnName("email");
        entity.Property(e => e.FirstNames)
            .HasMaxLength(160)
            .HasColumnName("first_names");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.FullName)
            .HasMaxLength(320)
            .HasColumnName("full_name");
        entity.Property(e => e.Gender)
            .HasMaxLength(40)
            .HasColumnName("gender");
        entity.Property(e => e.LastNames)
            .HasMaxLength(160)
            .HasColumnName("last_names");
        entity.Property(e => e.Phone)
            .HasMaxLength(40)
            .HasColumnName("phone");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.StudentCode)
            .HasMaxLength(80)
            .HasColumnName("student_code");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");
    }
}
