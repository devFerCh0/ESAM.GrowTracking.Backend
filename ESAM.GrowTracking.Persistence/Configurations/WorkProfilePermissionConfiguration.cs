using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class WorkProfilePermissionConfiguration : IEntityTypeConfiguration<WorkProfilePermission>
    {
        public void Configure(EntityTypeBuilder<WorkProfilePermission> builder)
        {
            builder.HasKey(wpp => new { wpp.WorkProfileId, wpp.PermissionId });
            builder.HasIndex(wpp => new { wpp.WorkProfileId, wpp.PermissionId }).IsUnique(true);
            builder.Property(wpp => wpp.HasAccess).IsRequired(true).HasDefaultValue(false);
            builder.Property(wpp => wpp.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(wpp => wpp.CreatedBy).IsRequired(true);
            builder.Property(wpp => wpp.UpdatedAt).IsRequired(false);
            builder.Property(wpp => wpp.UpdatedBy).IsRequired(false);
            builder.Property(wpp => wpp.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(wpp => wpp.Permission).WithMany(p => p.WorkProfilePermissions).HasForeignKey(wpp => wpp.PermissionId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(wpp => wpp.WorkProfile).WithMany(wp => wp.WorkProfilePermissions).HasForeignKey(wpp => wpp.WorkProfileId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}