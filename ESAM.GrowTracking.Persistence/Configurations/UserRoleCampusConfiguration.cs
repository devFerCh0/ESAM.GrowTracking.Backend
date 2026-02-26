using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class UserRoleCampusConfiguration : IEntityTypeConfiguration<UserRoleCampus>
    {
        public void Configure(EntityTypeBuilder<UserRoleCampus> builder)
        {
            builder.HasKey(urc => new { urc.UserId, urc.CampusId, urc.RoleId });
            builder.HasIndex(urc => new { urc.UserId, urc.CampusId, urc.RoleId }).IsUnique(true);
            builder.Property(urc => urc.IsDeleted).IsRequired(true).HasDefaultValue(false);
            builder.Property(urc => urc.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(urc => urc.CreatedBy).IsRequired(true);
            builder.Property(urc => urc.UpdatedAt).IsRequired(false);
            builder.Property(urc => urc.UpdatedBy).IsRequired(false);
            builder.Property(urc => urc.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(urc => urc.Campus).WithMany(c => c.UserRoleCampuses).HasForeignKey(urc => urc.CampusId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(urc => urc.Role).WithMany(r => r.UserRoleCampuses).HasForeignKey(urc => urc.RoleId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(urc => urc.User).WithMany(u => u.UserRoleCampuses).HasForeignKey(urc => urc.UserId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}