using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class SyncRunConfiguration : IEntityTypeConfiguration<SyncRun>
{
    public void Configure(EntityTypeBuilder<SyncRun> entity)
    {
        entity.HasKey(e => e.SyncRunId).HasName("sync_runs_pkey");

        entity.ToTable("sync_runs", "paquito");

        entity.HasIndex(e => new { e.EntityName, e.StartDate }, "ix_sync_runs_entity_date");

        entity.Property(e => e.SyncRunId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("sync_run_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.EndDate).HasColumnName("end_date");
        entity.Property(e => e.EntityName)
            .HasMaxLength(140)
            .HasColumnName("entity_name");
        entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
        entity.Property(e => e.ProcessedRecords).HasColumnName("processed_records");
        entity.Property(e => e.StartDate)
            .HasDefaultValueSql("now()")
            .HasColumnName("start_date");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .IsConcurrencyToken()
            .HasColumnName("status");
    }
}
