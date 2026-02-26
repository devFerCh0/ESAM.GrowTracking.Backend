using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();
            builder.HasIndex(p => p.FirstName).IsUnique(false);
            builder.Property(p => p.FirstName).IsRequired(true).HasMaxLength(100);
            builder.HasIndex(p => p.LastName).IsUnique(false);
            builder.Property(p => p.LastName).IsRequired(true).HasMaxLength(100);
            builder.HasIndex(p => p.SecondLastName).IsUnique(false);
            builder.Property(p => p.SecondLastName).IsRequired(false).HasMaxLength(100);
            builder.HasIndex(p => p.IdentityDocument).IsUnique(true);
            builder.Property(p => p.IdentityDocument).IsRequired(true).HasMaxLength(50);
            builder.HasIndex(p => p.IdentityDocumentType).IsUnique(false);
            builder.Property(p => p.IdentityDocumentType).HasConversion<byte>().IsRequired(true);
            builder.HasIndex(p => p.Gender).IsUnique(false);
            builder.Property(p => p.Gender).HasConversion<byte>().IsRequired(true);
            builder.HasIndex(p => p.MaritalStatus).IsUnique(false);
            builder.Property(p => p.MaritalStatus).HasConversion<byte>().IsRequired(true);
            builder.Property(p => p.IsDeleted).IsRequired(true).HasDefaultValue(false);
            builder.Property(p => p.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(p => p.CreatedBy).IsRequired(true);
            builder.Property(p => p.UpdatedAt).IsRequired(false);
            builder.Property(p => p.UpdatedBy).IsRequired(false);
            builder.Property(p => p.RecordVersion).IsRequired(true).IsRowVersion();
        }
    }
}