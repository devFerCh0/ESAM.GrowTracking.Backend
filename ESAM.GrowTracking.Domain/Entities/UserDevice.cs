using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Catalogs;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class UserDevice : AuditableEntity, IEntity<int>
    {
        private UserDevice() { }

        public int Id { get; private set; }

        public int UserId { get; private set; }

        public string DeviceIdentifier { get; private set; } = string.Empty;

        public string DeviceName { get; private set; } = string.Empty;

        public ApiClientType ApiClientType { get; private set; }

        public bool IsTrusted { get; private set; }

        public DateTime? LastSeenAt { get; private set; }

        public string? LastIp { get; private set; }

        public string? LastUserAgent { get; private set; }

        public bool IsDeleted { get; private set; }

        public int FailedLoginCount { get; private set; }

        public DateTime? LastFailedLoginAt { get; private set; }

        public DateTime? LockoutEndAt { get; private set; }

        public DateTime? LastLoginAt { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;

        public List<UserSession> UserSessions { get; private set; } = [];

        public UserDevice(int userId, string deviceIdentifier, string deviceName, ApiClientType apiClientType, string? lastIp, string? lastUserAgent, int createdBy)
        {
            UserId = userId;
            DeviceIdentifier = deviceIdentifier;
            DeviceName = deviceName;
            ApiClientType = apiClientType;
            LastIp = lastIp;
            LastUserAgent = lastUserAgent;
            CreateAudit(createdBy);
        }

        public void Update(string deviceName, ApiClientType apiClientType, string? lastIp, string? lastUserAgent)
        {
            DeviceName = deviceName;
            ApiClientType = apiClientType;
            LastIp = lastIp;
            LastUserAgent = lastUserAgent;
        }

        public void UpdateLastSeenAt(DateTime lastSeenAt)
        {
            LastSeenAt = lastSeenAt;
        }

        public bool IsLocked(DateTime utcNow) => LockoutEndAt.HasValue && LockoutEndAt.Value > utcNow;

        public bool ShouldResetFailedAttempts(TimeSpan duration, DateTime utcNow) => LastFailedLoginAt.HasValue && (utcNow - LastFailedLoginAt.Value) > duration;

        public void ResetFailedLogin()
        {
            FailedLoginCount = 0;
            LastFailedLoginAt = null;
            LockoutEndAt = null;
        }

        public void RegisterFailedLogin(int maxFailedAttempts, TimeSpan lockoutDuration, DateTime lastFailedLoginAt)
        {
            FailedLoginCount++;
            LastFailedLoginAt = lastFailedLoginAt;
            if (FailedLoginCount >= maxFailedAttempts)
                LockoutEndAt = lastFailedLoginAt.Add(lockoutDuration);
        }

        public void UpdateLastLogin(DateTime lastLoginAt)
        {
            LastLoginAt = lastLoginAt;
        }

        public void Activate()
        {
            IsDeleted = false;
        }
    }
}