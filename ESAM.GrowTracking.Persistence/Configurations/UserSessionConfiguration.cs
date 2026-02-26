using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.HasKey(us => us.Id);
            builder.Property(us => us.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(us => us.UserId).IsUnique(false);
            builder.Property(us => us.UserId).IsRequired(true);
            builder.HasIndex(us => us.UserDeviceId).IsUnique(false);
            builder.Property(us => us.UserDeviceId).IsRequired(true);
            builder.Property(us => us.IpAddress).IsRequired(false).HasMaxLength(50);
            builder.Property(us => us.UserAgent).IsRequired(false).HasMaxLength(512);
            builder.HasIndex(us => us.ExpiresAt).IsUnique(false);
            builder.Property(us => us.ExpiresAt).IsRequired(true);
            builder.HasIndex(us => us.AbsoluteExpiresAt).IsUnique(false);
            builder.Property(us => us.AbsoluteExpiresAt).IsRequired(true);
            builder.Property(us => us.LastActivityAt).IsRequired(false);
            builder.HasIndex(us => us.IsRevoked).IsUnique(false);
            builder.Property(us => us.IsRevoked).IsRequired(true).HasDefaultValue(false);
            builder.Property(us => us.RevokedAt).IsRequired(false);
            builder.Property(us => us.RevokedReason).IsRequired(false).HasMaxLength(512);
            builder.Property(us => us.IsPersistent).IsRequired(true);
            builder.HasIndex(us => us.ClosedByUserId).IsUnique(false);
            builder.Property(us => us.ClosedByUserId).IsRequired(false);
            builder.Property(us => us.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(us => us.CreatedBy).IsRequired(true);
            builder.Property(us => us.UpdatedAt).IsRequired(false);
            builder.Property(us => us.UpdatedBy).IsRequired(false);
            builder.Property(us => us.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(us => us.User).WithMany(u => u.UserSessions).HasForeignKey(us => us.UserId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(us => us.ClosedByUser).WithMany(u => u.SessionClosedByUsers).HasForeignKey(us => us.ClosedByUserId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(us => us.UserDevice).WithMany(ud => ud.UserSessions).HasForeignKey(us => us.UserDeviceId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}