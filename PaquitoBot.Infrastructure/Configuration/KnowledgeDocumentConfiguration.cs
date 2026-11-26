using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class KnowledgeDocumentConfiguration : IEntityTypeConfiguration<KnowledgeDocument>
{
    public void Configure(EntityTypeBuilder<KnowledgeDocument> entity)
    {
        entity.HasKey(e => e.DocumentId).HasName("knowledge_documents_pkey");

        entity.ToTable("knowledge_documents", "paquito");

        entity.HasIndex(e => e.CourseId, "ix_knowledge_documents_course");

        entity.Property(e => e.DocumentId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("document_id");
        entity.Property(e => e.Content).HasColumnName("content");
        entity.Property(e => e.ContentHash)
            .HasMaxLength(128)
            .HasColumnName("content_hash");
        entity.Property(e => e.CourseId).HasColumnName("course_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.DocumentType)
            .HasMaxLength(60)
            .HasColumnName("document_type");
        entity.Property(e => e.IndexedAt).HasColumnName("indexed_at");
        entity.Property(e => e.Source)
            .HasMaxLength(255)
            .HasColumnName("source");
        entity.Property(e => e.SourceUrl).HasColumnName("source_url");
        entity.Property(e => e.Status)
            .HasMaxLength(40)
            .HasDefaultValueSql("'active'::character varying")
            .HasColumnName("status");
        entity.Property(e => e.Title)
            .HasMaxLength(255)
            .HasColumnName("title");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Course).WithMany(p => p.KnowledgeDocuments)
            .HasForeignKey(d => d.CourseId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("knowledge_documents_course_id_fkey");
    }
}
