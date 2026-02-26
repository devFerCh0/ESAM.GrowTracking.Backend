using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class UserWorkProfileConfiguration : IEntityTypeConfiguration<UserWorkProfile>
    {
        public void Configure(EntityTypeBuilder<UserWorkProfile> builder)
        {
            builder.HasKey(uwp => new { uwp.UserId, uwp.WorkProfileId });
            builder.HasIndex(uwp => new { uwp.UserId, uwp.WorkProfileId }).IsUnique(true);
            builder.Property(uwp => uwp.IsDeleted).IsRequired(true).HasDefaultValue(false);
            builder.Property(uwp => uwp.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(uwp => uwp.CreatedBy).IsRequired(true);
            builder.Property(uwp => uwp.UpdatedAt).IsRequired(false);
            builder.Property(uwp => uwp.UpdatedBy).IsRequired(false);
            builder.Property(uwp => uwp.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(uwp => uwp.WorkProfile).WithMany(wp => wp.UserWorkProfiles).HasForeignKey(uwp => uwp.WorkProfileId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(uwp => uwp.User).WithMany(u => u.UserWorkProfiles).HasForeignKey(uwp => uwp.UserId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}