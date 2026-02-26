using ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IUserRoleCampusRepository : IRepository<UserRoleCampus>
    {
        Task<List<LoginUserRoleCampusProjection>> GetLoginUserRoleCampusesByUserIdAsync(int userId, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<UserRoleCampus?> GetByUserIdAndRoleIdAndCampusIdAsync(int userId, int roleId, int campusId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}