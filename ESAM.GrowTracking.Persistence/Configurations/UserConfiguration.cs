using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).IsRequired(true);
            builder.HasIndex(u => u.Username).IsUnique(true);
            builder.Property(u => u.Username).IsRequired(true).HasMaxLength(50);
            builder.HasIndex(u => u.NormalizedUserName).IsUnique(true);
            builder.Property(u => u.NormalizedUserName).IsRequired(true).HasMaxLength(50);
            builder.HasIndex(u => u.Email).IsUnique(true);
            builder.Property(u => u.Email).IsRequired(true).HasMaxLength(100);
            builder.HasIndex(u => u.NormalizedEmail).IsUnique(true);
            builder.Property(u => u.NormalizedEmail).IsRequired(true).HasMaxLength(100);
            builder.Property(u => u.Salt).IsRequired(true).HasMaxLength(128);
            builder.Property(u => u.PasswordHash).IsRequired(true).HasMaxLength(256);
            builder.Property(u => u.SecurityStamp).IsRequired(true).HasMaxLength(256).HasDefaultValue("NEWID()");
            builder.Property(u => u.TokenVersion).IsRequired(true).HasDefaultValue(0);
            builder.Property(u => u.IsEmailConfirmed).IsRequired(true).HasDefaultValue(false);
            builder.Property(u => u.IsDeleted).IsRequired(true).HasDefaultValue(false);
            builder.Property(u => u.LockoutEndAt).IsRequired(false);
            builder.Property(u => u.CreatedAt).IsRequired(true).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(u => u.CreatedBy).IsRequired(true);
            builder.Property(u => u.UpdatedAt).IsRequired(false);
            builder.Property(u => u.UpdatedBy).IsRequired(false);
            builder.Property(u => u.RecordVersion).IsRequired(true).IsRowVersion();
            builder.HasOne(u => u.Person).WithOne(p => p.User).HasForeignKey<User>(u => u.Id).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
        }
    }
}