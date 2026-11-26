using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class FeeScheduleConfiguration : IEntityTypeConfiguration<FeeSchedule>
{
    public void Configure(EntityTypeBuilder<FeeSchedule> entity)
    {
        entity.HasKey(e => e.FeeScheduleId).HasName("fee_schedules_pkey");

        entity.ToTable("fee_schedules", "paquito");

        entity.HasIndex(e => e.FrappeExternalId, "fee_schedules_frappe_external_id_key").IsUnique();

        entity.HasIndex(e => new { e.FeeStructureId, e.DueDate }, "ix_fee_schedules_structure_due");

        entity.Property(e => e.FeeScheduleId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("fee_schedule_id");
        entity.Property(e => e.AcademicPeriodId).HasColumnName("academic_period_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.DueDate).HasColumnName("due_date");
        entity.Property(e => e.FeeStructureId).HasColumnName("fee_structure_id");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.Name)
            .HasMaxLength(200)
            .HasColumnName("name");
        entity.Property(e => e.ScheduledAmount)
            .HasPrecision(14, 2)
            .HasColumnName("scheduled_amount");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.AcademicPeriod).WithMany(p => p.FeeSchedules)
            .HasForeignKey(d => d.AcademicPeriodId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fee_schedules_academic_period_id_fkey");

        entity.HasOne(d => d.FeeStructure).WithMany(p => p.FeeSchedules)
            .HasForeignKey(d => d.FeeStructureId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fee_schedules_fee_structure_id_fkey");
    }
}
