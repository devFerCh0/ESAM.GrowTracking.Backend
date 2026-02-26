using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IUserDeviceRepository : IRepository<UserDevice, int>
    {
        Task<UserDevice?> GetByUserIdAndDeviceIdentifierAsync(int userId, string deviceIdentifier, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<UserDevice?> GetByIdAndUserIdAsync(int id, int userId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}