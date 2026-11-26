using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class AcademicPeriodConfiguration : IEntityTypeConfiguration<AcademicPeriod>
{
    public void Configure(EntityTypeBuilder<AcademicPeriod> entity)
    {
        entity.HasKey(e => e.AcademicPeriodId).HasName("academic_periods_pkey");

        entity.ToTable("academic_periods", "paquito");

        entity.HasIndex(e => e.FrappeExternalId, "academic_periods_frappe_external_id_key").IsUnique();

        entity.Property(e => e.AcademicPeriodId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("academic_period_id");
        entity.Property(e => e.Code)
            .HasMaxLength(80)
            .HasColumnName("code");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.EndDate).HasColumnName("end_date");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.Name)
            .HasMaxLength(160)
            .HasColumnName("name");
        entity.Property(e => e.StartDate).HasColumnName("start_date");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");
    }
}
