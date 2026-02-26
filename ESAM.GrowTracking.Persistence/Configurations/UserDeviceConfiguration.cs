using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
    {
        public void Configure(EntityTypeBuilder<UserDevice> builder)
        {
            builder.HasKey(ud => ud.Id);
            builder.Property(ud => ud.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(ud => ud.UserId).IsUnique(false);
            builder.Property(ud => ud.UserId).IsRequired(true);
            builder.HasIndex(ud => ud.DeviceIdentifier).IsUnique(false);
            builder.Property(ud => ud.DeviceIdentifier).IsRequired(true).HasMaxLength(256);
            builder.Property(ud => ud.DeviceName).IsRequired(true).HasMaxLength(100);
            builder.HasIndex(ud => ud.ApiClientType).IsUnique(false);
            builder.Property(ud => ud.ApiClientType).HasConversion<byte>().IsRequired(true);
            builder.Property(ud => ud.IsTrusted).IsRequired(true).HasDefaultValue(false);
            builder.Property(ud => ud.LastSeenAt).IsRequired(false);
            builder.Property(ud => ud.LastIp).IsRequired(false).HasMaxLength(50);
            builder.Property(ud => ud.LastUserAgent).IsRequired(false).HasMaxLength(512);
            builder.Property(ud => ud.IsDeleted).IsRequired(true).HasDefaultValue(false);
            builder.Property(ud => ud.FailedLoginCount).IsRequired(true).HasDefaultValue(0);
            builder.Property(ud => ud.LastFailedLoginAt).IsRequired(false);
            builder.Property(ud => ud.LockoutEndAt).IsRequired(false);
            builder.Property(ud => ud.LastLoginAt).IsRequired(false);
            builder.Property(ud => ud.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(ud => ud.CreatedBy).IsRequired(true);
            builder.Property(ud => ud.UpdatedAt).IsRequired(false);
            builder.Property(ud => ud.UpdatedBy).IsRequired(false);
            builder.Property(ud => ud.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(ud => ud.User).WithMany(u => u.UserDevices).HasForeignKey(ud => ud.UserId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}