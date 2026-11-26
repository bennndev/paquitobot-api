using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> entity)
    {
        entity.HasKey(e => e.AssessmentId).HasName("assessments_pkey");

        entity.ToTable("assessments", "paquito");

        entity.HasIndex(e => e.FrappeExternalId, "assessments_frappe_external_id_key").IsUnique();

        entity.HasIndex(e => e.SectionId, "ix_assessments_section");

        entity.Property(e => e.AssessmentId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("assessment_id");
        entity.Property(e => e.AssessmentDate).HasColumnName("assessment_date");
        entity.Property(e => e.AssessmentType)
            .HasMaxLength(80)
            .HasColumnName("assessment_type");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.MaximumScore)
            .HasPrecision(10, 2)
            .HasColumnName("maximum_score");
        entity.Property(e => e.SectionId).HasColumnName("section_id");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasColumnName("status");
        entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
        entity.Property(e => e.Title)
            .HasMaxLength(255)
            .HasColumnName("title");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");
        entity.Property(e => e.Weight)
            .HasPrecision(10, 4)
            .HasColumnName("weight");

        entity.HasOne(d => d.Section).WithMany(p => p.Assessments)
            .HasForeignKey(d => d.SectionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("assessments_section_id_fkey");
    }
}
