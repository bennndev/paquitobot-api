using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> entity)
    {
        entity.HasKey(e => e.PaymentId).HasName("payments_pkey");

        entity.ToTable("payments", "paquito");

        entity.HasIndex(e => new { e.StudentId, e.PaidOn }, "ix_payments_student_paid");

        entity.HasIndex(e => e.TuitionFeeId, "ix_payments_tuition_fee");

        entity.HasIndex(e => e.FrappeExternalId, "payments_frappe_external_id_key").IsUnique();

        entity.Property(e => e.PaymentId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("payment_id");
        entity.Property(e => e.Amount)
            .HasPrecision(14, 2)
            .HasColumnName("amount");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.CurrencyCode)
            .HasMaxLength(3)
            .HasDefaultValueSql("'PEN'::character varying")
            .HasColumnName("currency_code");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.PaidOn).HasColumnName("paid_on");
        entity.Property(e => e.PaymentMethod)
            .HasMaxLength(80)
            .HasColumnName("payment_method");
        entity.Property(e => e.Reference)
            .HasMaxLength(140)
            .HasColumnName("reference");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.StudentId).HasColumnName("student_id");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.TuitionFeeId).HasColumnName("tuition_fee_id");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Student).WithMany(p => p.Payments)
            .HasForeignKey(d => d.StudentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("payments_student_id_fkey");

        entity.HasOne(d => d.TuitionFee).WithMany(p => p.Payments)
            .HasForeignKey(d => d.TuitionFeeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("payments_tuition_fee_id_fkey");
    }
}
