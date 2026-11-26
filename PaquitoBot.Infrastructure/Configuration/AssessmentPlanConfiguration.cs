using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class AssessmentPlanConfiguration : IEntityTypeConfiguration<AssessmentPlan>
{
    public void Configure(EntityTypeBuilder<AssessmentPlan> entity)
    {
        entity.HasKey(e => e.AssessmentPlanId).HasName("assessment_plans_pkey");

        entity.ToTable("assessment_plans", "paquito");

        entity.HasIndex(e => e.FrappeExternalId, "assessment_plans_frappe_external_id_key").IsUnique();

        entity.HasIndex(e => e.SectionId, "ix_assessment_plans_section");

        entity.Property(e => e.AssessmentPlanId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("assessment_plan_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.FrappeExternalId)
            .HasMaxLength(140)
            .HasColumnName("frappe_external_id");
        entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
        entity.Property(e => e.MaximumScore)
            .HasPrecision(10, 2)
            .HasColumnName("maximum_score");
        entity.Property(e => e.ScheduledDate).HasColumnName("scheduled_date");
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

        entity.HasOne(d => d.Section).WithMany(p => p.AssessmentPlans)
            .HasForeignKey(d => d.SectionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("assessment_plans_section_id_fkey");
    }
}
