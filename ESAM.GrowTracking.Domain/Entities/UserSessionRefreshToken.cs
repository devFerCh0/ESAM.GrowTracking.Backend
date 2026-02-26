using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class UserSessionRefreshToken : AuditableEntity, IEntity<int>
    {
        private UserSessionRefreshToken() { }

        public int Id { get; private set; }

        public int UserSessionId { get; private set; }

        public string TokenIdentifier { get; private set; } = string.Empty;

        public string Salt { get; private set; } = string.Empty;

        public string TokenHash { get; private set; } = string.Empty;

        public DateTime ExpiresAt { get; private set; }

        public DateTime? LastUsedAt { get; private set; }

        public int RotationCount { get; private set; }

        public int? ReplacedByUserSessionRefreshTokenId { get; private set; }

        public bool IsRevoked { get; private set; }

        public DateTime? RevokedAt { get; private set; }

        public string? RevokedReason { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public UserSession UserSession { get; private set; } = null!;

        public UserSessionRefreshToken? ReplacedByUserSessionRefreshToken { get; private set; }

        public UserSessionRefreshToken? ReplacesUserSessionRefreshToken { get; private set; }

        public List<BlacklistedRefreshToken> BlacklistedRefreshTokens { get; private set; } = [];

        public UserSessionRefreshToken(string tokenIdentifier, string salt, string tokenHash, DateTime expiresAt, int createdBy, int userSessionId = 0, int rotationCount = 0)
        {
            UserSessionId = userSessionId;
            TokenIdentifier = tokenIdentifier;
            Salt = salt;
            TokenHash = tokenHash;
            ExpiresAt = expiresAt;
            RotationCount = rotationCount;
            CreateAudit(createdBy);
        }

        public void UpdateLastUsedAt(DateTime lastUsedAt)
        {
            LastUsedAt = lastUsedAt;
        }

        public void AddUserSessionId(int userSessionId)
        {
            UserSessionId = userSessionId;
        }

        public void Revoke(DateTime revokedAt, string revokedReason)
        {
            IsRevoked = true;
            RevokedAt = revokedAt;
            RevokedReason = revokedReason;
        }

        public void UpdateReplacedByUserSessionRefreshTokenId(int replacedByUserSessionRefreshTokenId)
        {
            ReplacedByUserSessionRefreshTokenId = replacedByUserSessionRefreshTokenId;
        }
    }
}