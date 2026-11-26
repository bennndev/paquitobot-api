using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Configuration;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(e => e.UserId).HasName("users_pkey");

        entity.ToTable("users", "paquito");

        entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

        entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

        entity.Property(e => e.UserId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("user_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Email)
            .HasMaxLength(254)
            .HasColumnName("email");
        entity.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");
        entity.Property(e => e.PasswordHash)
            .HasMaxLength(255)
            .HasColumnName("password_hash");
        entity.Property(e => e.Role)
            .HasMaxLength(40)
            .HasDefaultValueSql("'student'::character varying")
            .HasColumnName("role");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");
        entity.Property(e => e.Username)
            .HasMaxLength(100)
            .HasColumnName("username");
    }
}
