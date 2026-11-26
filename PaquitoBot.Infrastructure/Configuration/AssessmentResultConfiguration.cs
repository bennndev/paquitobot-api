using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class AssessmentResultConfiguration : IEntityTypeConfiguration<AssessmentResult>
{
    public void Configure(EntityTypeBuilder<AssessmentResult> entity)
    {
        entity.HasKey(e => e.ResultId).HasName("assessment_results_pkey");

        entity.ToTable("assessment_results", "paquito");

        entity.HasIndex(e => e.FrappeExternalId, "assessment_results_frappe_external_id_key").IsUnique();

        entity.HasIndex(e => new { e.StudentId, e.AssessmentId }, "ix_assessment_results_student");

        entity.Property(e => e.ResultId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("result_id");
        entity.Property(e => e.AssessmentId).HasColumnName("assessment_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.Grade)
            .HasPrecision(10, 2)
            .HasColumnName("grade");
        entity.Property(e => e.Notes).HasColumnName("notes");
        entity.Property(e => e.StudentId).HasColumnName("student_id");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Assessment).WithMany(p => p.AssessmentResults)
            .HasForeignKey(d => d.AssessmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("assessment_results_assessment_id_fkey");

        entity.HasOne(d => d.Student).WithMany(p => p.AssessmentResults)
            .HasForeignKey(d => d.StudentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("assessment_results_student_id_fkey");
    }
}
