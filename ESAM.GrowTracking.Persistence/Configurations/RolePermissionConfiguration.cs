using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique(true);
            builder.Property(rp => rp.HasAccess).IsRequired(true).HasDefaultValue(false);
            builder.Property(rp => rp.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(rp => rp.CreatedBy).IsRequired(true);
            builder.Property(rp => rp.UpdatedAt).IsRequired(false);
            builder.Property(rp => rp.UpdatedBy).IsRequired(false);
            builder.Property(rp => rp.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}