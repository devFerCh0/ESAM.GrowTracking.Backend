using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IBlacklistedRefreshTokenRepository : IRepository<BlacklistedRefreshToken, int>
    {
        Task<List<string>> GetExistingTokenIdentifiersAsync(List<string> tokenIdentifiers, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string tokenIdentifier, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<int> PurgeExpiredBlacklistedRefreshTokensAsync(int batchSize, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}