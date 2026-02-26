using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class UserPhotoConfiguration : IEntityTypeConfiguration<UserPhoto>
    {
        public void Configure(EntityTypeBuilder<UserPhoto> builder)
        {
            builder.HasKey(up => up.Id);
            builder.Property(up => up.Id).IsRequired(true).ValueGeneratedOnAdd();
            builder.HasIndex(up => up.UserId).IsUnique(false);
            builder.Property(up => up.UserId).IsRequired(true);
            builder.Property(up => up.Photo).IsRequired(true).HasMaxLength(512);
            builder.Property(up => up.IsDefault).IsRequired(true).HasDefaultValue(false);
            builder.Property(up => up.IsDeleted).IsRequired(true).HasDefaultValue(false);
            builder.Property(up => up.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(up => up.CreatedBy).IsRequired(true);
            builder.Property(up => up.UpdatedAt).IsRequired(false);
            builder.Property(up => up.UpdatedBy).IsRequired(false);
            builder.Property(up => up.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(up => up.User).WithMany(u => u.UserPhotos).HasForeignKey(up => up.UserId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}