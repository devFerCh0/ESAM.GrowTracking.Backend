using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(p => p.ModuleId).IsUnique(false);
            builder.Property(p => p.ModuleId).IsRequired(true);
            builder.HasIndex(p => p.Name).IsUnique(true);
            builder.Property(p => p.Name).IsRequired(true).HasMaxLength(100);
            builder.Property(p => p.Description).IsRequired(false).HasMaxLength(250);
            builder.HasIndex(p => p.Code).IsUnique(true);
            builder.Property(p => p.Code).IsRequired(false).HasMaxLength(50);
            builder.Property(p => p.IsDeleted).IsRequired(true).HasDefaultValue(false);
            builder.HasOne(p => p.Module).WithMany(m => m.Permissions).HasForeignKey(p => p.ModuleId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}