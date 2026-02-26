using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IUserWorkProfileRepository : IRepository<UserWorkProfile>
    {
        Task<UserWorkProfile?> GetByUserIdAndWorkProfileIdAsync(int userId, int workProfileId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}