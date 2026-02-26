using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(r => r.Name).IsUnique(true);
            builder.Property(r => r.Name).IsRequired(true).HasMaxLength(100);
            builder.Property(r => r.Description).IsRequired(false).HasMaxLength(250);
            builder.Property(r => r.IsDeleted).IsRequired(true).HasDefaultValue(false);
            builder.Property(r => r.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(r => r.CreatedBy).IsRequired(true);
            builder.Property(r => r.UpdatedAt).IsRequired(false);
            builder.Property(r => r.UpdatedBy).IsRequired(false);
            builder.Property(r => r.RecordVersion).IsRequired(true).IsRowVersion();
        }
    }
}