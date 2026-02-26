using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class UserSessionRefreshTokenConfiguration : IEntityTypeConfiguration<UserSessionRefreshToken>
    {
        public void Configure(EntityTypeBuilder<UserSessionRefreshToken> builder)
        {
            builder.HasKey(ust => ust.Id);
            builder.Property(ust => ust.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(ust => ust.UserSessionId).IsUnique(false);
            builder.Property(ust => ust.UserSessionId).IsRequired(true);
            builder.HasIndex(ust => ust.TokenIdentifier).IsUnique(true);
            builder.Property(ust => ust.TokenIdentifier).IsRequired(true).HasMaxLength(256);
            builder.Property(ust => ust.Salt).IsRequired(true).HasMaxLength(256);
            builder.Property(ust => ust.TokenHash).IsRequired(true).HasMaxLength(512);
            builder.HasIndex(ust => ust.ExpiresAt).IsUnique(false);
            builder.Property(ust => ust.ExpiresAt).IsRequired(true);
            builder.Property(ust => ust.LastUsedAt).IsRequired(false);
            builder.Property(ust => ust.RotationCount).IsRequired(true);
            builder.HasIndex(ust => ust.ReplacedByUserSessionRefreshTokenId).IsUnique(true);
            builder.Property(ust => ust.ReplacedByUserSessionRefreshTokenId).IsRequired(false);
            builder.HasIndex(ust => ust.IsRevoked).IsUnique(false);
            builder.Property(ust => ust.IsRevoked).IsRequired(true).HasDefaultValue(false);
            builder.Property(ust => ust.RevokedAt).IsRequired(false);
            builder.Property(ust => ust.RevokedReason).IsRequired(false).HasMaxLength(512);
            builder.Property(ust => ust.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(ust => ust.CreatedBy).IsRequired(true);
            builder.Property(ust => ust.UpdatedAt).IsRequired(false);
            builder.Property(ust => ust.UpdatedBy).IsRequired(false);
            builder.Property(ust => ust.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(ust => ust.UserSession).WithMany(us => us.UserSessionRefreshTokens).HasForeignKey(ust => ust.UserSessionId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(ust => ust.ReplacedByUserSessionRefreshToken).WithOne(ust => ust.ReplacesUserSessionRefreshToken).HasForeignKey<UserSessionRefreshToken>(ust => ust.ReplacedByUserSessionRefreshTokenId)
                .IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        }
    }
}