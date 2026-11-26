using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> entity)
    {
        entity.HasKey(e => e.MessageId).HasName("messages_pkey");

        entity.ToTable("messages", "paquito");

        entity.HasIndex(e => new { e.ConversationId, e.CreatedAt }, "ix_messages_conversation_date");

        entity.Property(e => e.MessageId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("message_id");
        entity.Property(e => e.Content).HasColumnName("content");
        entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.SenderType)
            .HasMaxLength(40)
            .HasColumnName("sender_type");

        entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
            .HasForeignKey(d => d.ConversationId)
            .HasConstraintName("messages_conversation_id_fkey");
    }
}
