using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class FeeStructureConfiguration : IEntityTypeConfiguration<FeeStructure>
{
    public void Configure(EntityTypeBuilder<FeeStructure> entity)
    {
        entity.HasKey(e => e.FeeStructureId).HasName("fee_structures_pkey");

        entity.ToTable("fee_structures", "paquito");

        entity.HasIndex(e => e.FrappeExternalId, "fee_structures_frappe_external_id_key").IsUnique();

        entity.HasIndex(e => e.ProgramId, "ix_fee_structures_program");

        entity.Property(e => e.FeeStructureId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("fee_structure_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.CurrencyCode)
            .HasMaxLength(3)
            .HasDefaultValueSql("'PEN'::character varying")
            .HasColumnName("currency_code");
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.Name)
            .HasMaxLength(200)
            .HasColumnName("name");
        entity.Property(e => e.ProgramId).HasColumnName("program_id");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.TotalAmount)
            .HasPrecision(14, 2)
            .HasColumnName("total_amount");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Program).WithMany(p => p.FeeStructures)
            .HasForeignKey(d => d.ProgramId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fee_structures_program_id_fkey");
    }
}
