using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IWorkProfilePermissionRepository : IRepository<WorkProfilePermission>
    {
        Task<bool> HasActivePermissionsAsync(int workProfileId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}