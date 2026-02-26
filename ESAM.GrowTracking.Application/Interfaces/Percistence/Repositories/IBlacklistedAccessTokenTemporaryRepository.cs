using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IBlacklistedAccessTokenTemporaryRepository : IRepository<BlacklistedAccessTokenTemporary, int>
    {
        Task<bool> ExistsAsync(string jti, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<int> PurgeExpiredBlacklistedAccessTokensTemporaryAsync(int batchSize, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}