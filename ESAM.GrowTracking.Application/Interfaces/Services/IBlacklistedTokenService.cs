using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Services
{
    public interface IBlacklistedTokenService
    {
        Task<BlacklistedAccessTokenTemporary?> TryGenerateBlacklistedAccessTokenTemporaryAsync(int userId, string jti, DateTime expirationDate, DateTime blacklistedAt, string reason, int createdBy, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<BlacklistedAccessTokenPermanent?> TryGenerateBlacklistedAccessTokenPermanentAsync(int userSessionId, string jti, DateTime expirationDate, DateTime blacklistedAt, string reason, int createdBy, 
            bool asTracking = false, CancellationToken cancellationToken = default);

        Task<BlacklistedRefreshToken?> TryGenerateBlacklistedRefreshTokenAsync(int userSessionRefreshTokenId, string tokenIdentifier, DateTime expirationDate, DateTime blacklistedAt, string reason, int createdBy, 
            bool asTracking = false, CancellationToken cancellationToken = default);

        Task<List<BlacklistedRefreshToken>> GetPendingBlacklistedRefreshTokensAsync(List<(int Id, string TokenIdentifier, DateTime ExpiresAt)> userSessionRefreshTokens, DateTime blacklistedAt, string reason, 
            int currentUserId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}