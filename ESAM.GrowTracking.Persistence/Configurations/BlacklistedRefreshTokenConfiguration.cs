using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class BlacklistedRefreshTokenConfiguration : IEntityTypeConfiguration<BlacklistedRefreshToken>
    {
        public void Configure(EntityTypeBuilder<BlacklistedRefreshToken> builder)
        {
            builder.HasKey(brt => brt.Id);
            builder.Property(brt => brt.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(brt => brt.UserSessionRefreshTokenId).IsUnique(true);
            builder.Property(brt => brt.UserSessionRefreshTokenId).IsRequired(true);
            builder.HasIndex(brt => brt.TokenIdentifier).IsUnique(false);
            builder.Property(brt => brt.TokenIdentifier).IsRequired(true).HasMaxLength(256);
            builder.HasIndex(brt => brt.ExpirationDate).IsUnique(false);
            builder.Property(brt => brt.ExpirationDate).IsRequired(true);
            builder.Property(brt => brt.BlacklistedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(brt => brt.Reason).IsRequired(false).HasMaxLength(250);
            builder.Property(brt => brt.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(brt => brt.CreatedBy).IsRequired(false);
            builder.Property(brt => brt.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(brt => brt.UserSessionRefreshToken).WithMany(ust => ust.BlacklistedRefreshTokens).HasForeignKey(brt => brt.UserSessionRefreshTokenId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}