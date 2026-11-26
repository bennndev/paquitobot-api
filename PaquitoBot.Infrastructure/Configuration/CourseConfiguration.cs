using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> entity)
    {
        entity.HasKey(e => e.CourseId).HasName("courses_pkey");

        entity.ToTable("courses", "paquito");

        entity.HasIndex(e => e.FrappeExternalId, "courses_frappe_external_id_key").IsUnique();

        entity.HasIndex(e => e.FrappeModifiedAt, "ix_courses_sync");

        entity.Property(e => e.CourseId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("course_id");
        entity.Property(e => e.Code)
            .HasMaxLength(80)
            .HasColumnName("code");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Credits).HasColumnName("credits");
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.Name)
            .HasMaxLength(255)
            .HasColumnName("name");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");
    }
}
