using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class BlacklistedAccessTokenTemporaryConfiguration : IEntityTypeConfiguration<BlacklistedAccessTokenTemporary>
    {
        public void Configure(EntityTypeBuilder<BlacklistedAccessTokenTemporary> builder)
        {
            builder.HasKey(batt => batt.Id);
            builder.Property(batt => batt.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(batt => batt.UserId).IsUnique(false);
            builder.Property(batt => batt.UserId).IsRequired(true);
            builder.HasIndex(batt => batt.Jti).IsUnique(false);
            builder.Property(batt => batt.Jti).IsRequired(true).HasMaxLength(256);
            builder.HasIndex(batt => batt.ExpirationDate).IsUnique(false);
            builder.Property(batt => batt.ExpirationDate).IsRequired(true);
            builder.Property(batt => batt.BlacklistedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(batt => batt.Reason).IsRequired(false).HasMaxLength(250);
            builder.Property(batt => batt.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(batt => batt.CreatedBy).IsRequired(false);
            builder.Property(batt => batt.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(batt => batt.User).WithMany(u => u.BlacklistedAccessTokensTemporary).HasForeignKey(batt => batt.UserId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}