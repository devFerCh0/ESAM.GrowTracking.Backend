using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IBlacklistedAccessTokenPermanentRepository : IRepository<BlacklistedAccessTokenPermanent, int>
    {
        Task<bool> ExistsAsync(string jti, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<int> PurgeExpiredBlacklistedAccessTokensPermanentAsync(int batchSize, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}