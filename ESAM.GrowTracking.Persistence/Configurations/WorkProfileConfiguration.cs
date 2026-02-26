using ESAM.GrowTracking.Domain.Catalogs;
using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class WorkProfileConfiguration : IEntityTypeConfiguration<WorkProfile>
    {
        public void Configure(EntityTypeBuilder<WorkProfile> builder)
        {
            builder.HasKey(wp => wp.Id);
            builder.Property(wp => wp.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(wp => wp.Name).IsUnique(true);
            builder.Property(wp => wp.Name).IsRequired(true).HasMaxLength(100);
            builder.Property(wp => wp.Description).IsRequired(false).HasMaxLength(250);
            builder.HasIndex(wp => wp.WorkProfileType).IsUnique(false);
            builder.Property(wp => wp.WorkProfileType).HasConversion<byte>().IsRequired(true).HasDefaultValue(WorkProfileType.None);
            builder.Property(wp => wp.IsDeleted).IsRequired(true).HasDefaultValue(false);
        }
    }
}