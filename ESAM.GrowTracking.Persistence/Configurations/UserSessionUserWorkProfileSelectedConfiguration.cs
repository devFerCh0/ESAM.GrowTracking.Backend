using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class UserSessionUserWorkProfileSelectedConfiguration : IEntityTypeConfiguration<UserSessionUserWorkProfileSelected>
    {
        public void Configure(EntityTypeBuilder<UserSessionUserWorkProfileSelected> builder)
        {
            builder.HasKey(usuwps => usuwps.UserSessionId);
            builder.Property(usuwps => usuwps.UserSessionId).IsRequired(true);
            builder.HasOne(usuwps => usuwps.UserSession).WithOne(us => us.UserSessionUserWorkProfileSelected).HasForeignKey<UserSessionUserWorkProfileSelected>(uswps => uswps.UserSessionId).IsRequired(true)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(usuwps => usuwps.UserWorkProfile).WithMany(uwp => uwp.UserSessionUserWorkProfilesSelected).HasForeignKey(usuwps => new { usuwps.UserId, usuwps.WorkProfileId }).IsRequired(true)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}