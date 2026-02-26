using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class BlacklistedAccessTokenPermanent : IEntity<int>
    {
        private BlacklistedAccessTokenPermanent() { }

        public int Id { get; private set; }

        public int UserSessionId { get; private set; }

        public string Jti { get; private set; } = string.Empty;

        public DateTime ExpirationDate { get; private set; }

        public DateTime BlacklistedAt { get; private set; }

        public string? Reason { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public int? CreatedBy { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public UserSession UserSession { get; private set; } = null!;

        public BlacklistedAccessTokenPermanent(int userSessionId, string jti, DateTime expirationDate, DateTime blacklistedAt, string reason, int createdBy)
        {
            UserSessionId = userSessionId;
            Jti = jti;
            ExpirationDate = expirationDate;
            BlacklistedAt = blacklistedAt;
            Reason = reason;
            CreatedBy = createdBy;
        }
    }
}