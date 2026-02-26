using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class User : AuditableEntity, IEntity<int>
    {
        private User() { }

        public int Id { get; private set; }

        public string Username { get; private set; } = string.Empty;

        public string NormalizedUserName { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string NormalizedEmail { get; private set; } = string.Empty;

        public string Salt { get; private set; } = string.Empty;

        public string PasswordHash { get; private set; } = string.Empty;

        public string SecurityStamp { get; private set; } = string.Empty;

        public int TokenVersion { get; private set; }

        public bool IsEmailConfirmed { get; private set; }

        public bool IsDeleted { get; private set; }

        public DateTime? LockoutEndAt { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public Person Person { get; private set; } = null!;

        public List<UserWorkProfile> UserWorkProfiles { get; private set; } = [];

        public List<UserRoleCampus> UserRoleCampuses { get; private set; } = [];

        public List<BlacklistedAccessTokenTemporary> BlacklistedAccessTokensTemporary { get; private set; } = [];

        public List<UserSession> UserSessions { get; private set; } = [];

        public List<UserSession>? SessionClosedByUsers { get; private set; }

        public List<UserPhoto> UserPhotos { get; private set; } = [];

        public List<UserDevice> UserDevices { get; private set; } = [];

        public User(int id, string username, string email, string salt, string passwordHash, string securityStamp, int createdBy)
        {
            Id = id;
            Username = username;
            NormalizedUserName = username.ToUpperInvariant();
            Email = email;
            NormalizedEmail = email.ToUpperInvariant();
            Salt = salt;
            PasswordHash = passwordHash;
            SecurityStamp = securityStamp;
            CreateAudit(createdBy);
        }

        public bool IsLocked(DateTime utcNow) => LockoutEndAt.HasValue && LockoutEndAt.Value > utcNow;
    }
}