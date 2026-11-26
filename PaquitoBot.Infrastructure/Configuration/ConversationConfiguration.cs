using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> entity)
    {
        entity.HasKey(e => e.ConversationId).HasName("conversations_pkey");

        entity.ToTable("conversations", "paquito");

        entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "ix_conversations_user_date");

        entity.Property(e => e.ConversationId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("conversation_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
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
        entity.Property(e => e.UserId).HasColumnName("user_id");

        entity.HasOne(d => d.User).WithMany(p => p.Conversations)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("conversations_user_id_fkey");
    }
}
