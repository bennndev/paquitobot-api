using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class TuitionFeeConfiguration : IEntityTypeConfiguration<TuitionFee>
{
    public void Configure(EntityTypeBuilder<TuitionFee> entity)
    {
        entity.HasKey(e => e.TuitionFeeId).HasName("tuition_fees_pkey");

        entity.ToTable("tuition_fees", "paquito");

        entity.HasIndex(e => e.FeeScheduleId, "ix_tuition_fees_schedule");

        entity.HasIndex(e => new { e.StudentId, e.DueDate }, "ix_tuition_fees_student_due");

        entity.HasIndex(e => e.FrappeExternalId, "tuition_fees_frappe_external_id_key").IsUnique();

        entity.Property(e => e.TuitionFeeId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("tuition_fee_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.CurrencyCode)
            .HasMaxLength(3)
            .HasDefaultValueSql("'PEN'::character varying")
            .HasColumnName("currency_code");
        entity.Property(e => e.Description)
            .HasMaxLength(255)
            .HasColumnName("description");
        entity.Property(e => e.DueDate).HasColumnName("due_date");
        entity.Property(e => e.FeeScheduleId).HasColumnName("fee_schedule_id");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.IssuedOn).HasColumnName("issued_on");
        entity.Property(e => e.OriginalAmount)
            .HasPrecision(14, 2)
            .HasColumnName("original_amount");
        entity.Property(e => e.OutstandingBalance)
            .HasPrecision(14, 2)
            .HasColumnName("outstanding_balance");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.StudentId).HasColumnName("student_id");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.FeeSchedule).WithMany(p => p.TuitionFees)
            .HasForeignKey(d => d.FeeScheduleId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("tuition_fees_fee_schedule_id_fkey");

        entity.HasOne(d => d.Student).WithMany(p => p.TuitionFees)
            .HasForeignKey(d => d.StudentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("tuition_fees_student_id_fkey");
    }
}
