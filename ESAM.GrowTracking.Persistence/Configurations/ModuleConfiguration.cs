using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(m => m.Name).IsUnique(true);
            builder.Property(m => m.Name).IsRequired(true).HasMaxLength(100);
            builder.Property(m => m.Description).IsRequired(false).HasMaxLength(250);
            builder.Property(m => m.IsDeleted).IsRequired(true).HasDefaultValue(false);
        }
    }
}