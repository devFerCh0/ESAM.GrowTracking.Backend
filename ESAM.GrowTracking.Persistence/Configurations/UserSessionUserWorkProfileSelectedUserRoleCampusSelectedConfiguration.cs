using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class UserSessionUserWorkProfileSelectedUserRoleCampusSelectedConfiguration : IEntityTypeConfiguration<UserSessionUserWorkProfileSelectedUserRoleCampusSelected>
    {

        public void Configure(EntityTypeBuilder<UserSessionUserWorkProfileSelectedUserRoleCampusSelected> builder)
        {
            builder.HasKey(usuwpsurcs => usuwpsurcs.UserSessionId);
            builder.Property(usuwpsurcs => usuwpsurcs.UserSessionId).IsRequired(true);
            builder.HasOne(usuwpsurcs => usuwpsurcs.UserSessionUserWorkProfileSelected).WithOne(usuwps => usuwps.UserSessionUserWorkProfileSelectedUserRoleCampusSelected)
                .HasForeignKey<UserSessionUserWorkProfileSelectedUserRoleCampusSelected>(usuwpsurcs => usuwpsurcs.UserSessionId).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(usuwpsurcs => usuwpsurcs.UserRoleCampus).WithMany(urc => urc.UserSessionUserWorkProfileSelectedUserRoleCampusSelected)
                .HasForeignKey(usuwpsurcs => new { usuwpsurcs.UserId, usuwpsurcs.RoleId, usuwpsurcs.CampusId }).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}