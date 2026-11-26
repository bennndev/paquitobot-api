using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> entity)
    {
        entity.HasKey(e => e.InstructorId).HasName("instructors_pkey");

        entity.ToTable("instructors", "paquito");

        entity.HasIndex(e => e.FrappeExternalId, "instructors_frappe_external_id_key").IsUnique();

        entity.Property(e => e.InstructorId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("instructor_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Email)
            .HasMaxLength(254)
            .HasColumnName("email");
        entity.Property(e => e.FirstName)
            .HasMaxLength(160)
            .HasColumnName("first_name");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.FullName)
            .HasMaxLength(320)
            .HasColumnName("full_name");
        entity.Property(e => e.LastName)
            .HasMaxLength(160)
            .HasColumnName("last_name");
        entity.Property(e => e.Phone)
            .HasMaxLength(40)
            .HasColumnName("phone");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");
    }
}
