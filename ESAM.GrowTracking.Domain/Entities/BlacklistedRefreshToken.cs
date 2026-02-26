using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class BlacklistedRefreshToken : IEntity<int>
    {
        private BlacklistedRefreshToken() { }

        public int Id { get; private set; }

        public int UserSessionRefreshTokenId { get; private set; }

        public string TokenIdentifier { get; private set; } = string.Empty;

        public DateTime ExpirationDate { get; private set; }

        public DateTime BlacklistedAt { get; private set; }

        public string? Reason { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public int? CreatedBy { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public UserSessionRefreshToken UserSessionRefreshToken { get; private set; } = null!;

        public BlacklistedRefreshToken(int userSessionRefreshTokenId, string tokenIdentifier, DateTime expirationDate, DateTime blacklistedAt, string reason, int createdBy)
        {
            UserSessionRefreshTokenId = userSessionRefreshTokenId;
            TokenIdentifier = tokenIdentifier;
            ExpirationDate = expirationDate;
            BlacklistedAt = blacklistedAt;
            Reason = reason;
            CreatedBy = createdBy;
        }
    }
}