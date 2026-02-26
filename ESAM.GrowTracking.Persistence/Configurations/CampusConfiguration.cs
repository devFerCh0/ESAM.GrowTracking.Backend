using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class CampusConfiguration : IEntityTypeConfiguration<Campus>
    {
        public void Configure(EntityTypeBuilder<Campus> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(c => c.BusinessUnitId).IsUnique(false);
            builder.Property(c => c.BusinessUnitId).IsRequired(true);
            builder.HasIndex(c => c.Name).IsUnique(true);
            builder.Property(c => c.Name).IsRequired(true).HasMaxLength(150);
            builder.HasIndex(c => c.WebSite).IsUnique(true);
            builder.Property(c => c.WebSite).IsRequired(true).HasMaxLength(250);
            builder.Property(c => c.FoundingDate).IsRequired(true).HasColumnType("date");
            builder.Property(c => c.IsDeleted).IsRequired(true).HasDefaultValue(false);
            builder.Property(c => c.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(c => c.CreatedBy).IsRequired(true);
            builder.Property(c => c.UpdatedAt).IsRequired(false);
            builder.Property(c => c.UpdatedBy).IsRequired(false);
            builder.Property(c => c.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(c => c.BusinessUnit).WithMany(bu => bu.Campuses).HasForeignKey(c => c.BusinessUnitId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}