using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class BlacklistedAccessTokenPermanentConfiguration : IEntityTypeConfiguration<BlacklistedAccessTokenPermanent>
    {
        public void Configure(EntityTypeBuilder<BlacklistedAccessTokenPermanent> builder)
        {
            builder.HasKey(batp => batp.Id);
            builder.Property(batp => batp.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(batp => batp.UserSessionId).IsUnique(false);
            builder.Property(batp => batp.UserSessionId).IsRequired(true);
            builder.HasIndex(batp => batp.Jti).IsUnique(false);
            builder.Property(batp => batp.Jti).IsRequired(true).HasMaxLength(256);
            builder.HasIndex(batp => batp.ExpirationDate).IsUnique(false);
            builder.Property(batp => batp.ExpirationDate).IsRequired(true);
            builder.Property(batp => batp.BlacklistedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(batp => batp.Reason).IsRequired(false).HasMaxLength(250);
            builder.Property(batp => batp.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(batp => batp.CreatedBy).IsRequired(false);
            builder.Property(batp => batp.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(batp => batp.UserSession).WithMany(us => us.BlacklistedAccessTokensPermanent).HasForeignKey(batp => batp.UserSessionId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}