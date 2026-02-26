using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class UserSession : AuditableEntity, IEntity<int>
    {
        private UserSession() { }

        public int Id { get; private set; }

        public int UserId { get; private set; }

        public int UserDeviceId { get; private set; }

        public string? IpAddress { get; private set; }

        public string? UserAgent { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public DateTime AbsoluteExpiresAt { get; private set; }

        public DateTime? LastActivityAt { get; private set; }

        public bool IsRevoked { get; private set; }

        public DateTime? RevokedAt { get; private set; }

        public string? RevokedReason { get; private set; }

        public bool IsPersistent { get; private set; }

        public int? ClosedByUserId { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;

        public UserDevice UserDevice { get; private set; } = null!;

        public UserSessionUserWorkProfileSelected UserSessionUserWorkProfileSelected { get; private set; } = null!;

        public User? ClosedByUser { get; private set; }

        public List<UserSessionRefreshToken> UserSessionRefreshTokens { get; private set; } = [];

        public List<BlacklistedAccessTokenPermanent> BlacklistedAccessTokensPermanent { get; private set; } = [];

        public UserSession(int userId, int userDeviceId, string? ipAddress, string? userAgent, DateTime expiresAt, DateTime absoluteExpiresAt, bool isPersistent, int createdBy)
        {
            UserId = userId;
            UserDeviceId = userDeviceId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            ExpiresAt = expiresAt;
            AbsoluteExpiresAt = absoluteExpiresAt;
            IsPersistent = isPersistent;
            CreateAudit(createdBy);
        }

        public void UpdateLastActivity(DateTime lastActivityAt)
        {
            LastActivityAt = lastActivityAt;
        }

        public void Revoke(DateTime revokedAt, string revokedReason, int closedByUserId)
        {
            IsRevoked = true;
            RevokedAt = revokedAt;
            RevokedReason = revokedReason;
            ClosedByUserId = closedByUserId;
        }

        public void UpdateExpiresAt(DateTime expiresAt)
        {
            ExpiresAt = expiresAt;
        }
    }
}