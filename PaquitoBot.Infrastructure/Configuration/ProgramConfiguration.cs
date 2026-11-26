using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;
using Program = PaquitoBot.Domain.Models.Program;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class ProgramConfiguration : IEntityTypeConfiguration<Program>
{
    public void Configure(EntityTypeBuilder<Program> entity)
    {
        entity.HasKey(e => e.ProgramId).HasName("programs_pkey");

        entity.ToTable("programs", "paquito");

        entity.HasIndex(e => e.FrappeModifiedAt, "ix_programs_sync");

        entity.HasIndex(e => e.FrappeExternalId, "programs_frappe_external_id_key").IsUnique();

        entity.Property(e => e.ProgramId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("program_id");
        entity.Property(e => e.Code)
            .HasMaxLength(80)
            .HasColumnName("code");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
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
