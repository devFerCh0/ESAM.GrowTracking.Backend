using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        Task<bool> HasActivePermissionsAsync(int roleId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}