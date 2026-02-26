using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IUserSessionRepository : IRepository<UserSession, int>
    {
        Task<UserSession?> GetByIdAndUserIdAnduserDeviceIdAsync(int id, int userId, int userDeviceId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}