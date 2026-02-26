using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class BusinessUnitConfiguration : IEntityTypeConfiguration<BusinessUnit>
    {
        public void Configure(EntityTypeBuilder<BusinessUnit> builder)
        {
            builder.HasKey(bu => bu.Id);
            builder.Property(bu => bu.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(bu => bu.Name).IsUnique(true);
            builder.Property(bu => bu.Name).IsRequired(true).HasMaxLength(100);
            builder.HasIndex(bu => bu.Abbreviation).IsUnique(true);
            builder.Property(bu => bu.Abbreviation).IsRequired(true).HasMaxLength(10);
            builder.HasIndex(bu => bu.WebSite).IsUnique(true);
            builder.Property(bu => bu.WebSite).IsRequired(true).HasMaxLength(256);
            builder.Property(bu => bu.FoundingDate).IsRequired(true).HasColumnType("date");
            builder.Property(bu => bu.IsDeleted).IsRequired(true).HasDefaultValue(false);
            builder.Property(bu => bu.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(bu => bu.CreatedBy).IsRequired(true);
            builder.Property(bu => bu.UpdatedAt).IsRequired(false);
            builder.Property(bu => bu.UpdatedBy).IsRequired(false);
            builder.Property(bu => bu.RecordVersion).IsRequired(true).IsRowVersion();
        }
    }
}