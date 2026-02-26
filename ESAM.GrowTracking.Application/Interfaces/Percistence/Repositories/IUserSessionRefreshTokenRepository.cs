using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IUserSessionRefreshTokenRepository : IRepository<UserSessionRefreshToken, int>
    {
        Task<UserSessionRefreshToken?> GetByTokenIdentifierAsync(string tokenIdentifier, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<List<UserSessionRefreshToken>> GetAllByUserSessionIdAsync(int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<int> PurgeExpiredUserSessionRefreshTokensAsync(int batchSize, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}