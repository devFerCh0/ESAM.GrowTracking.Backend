using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IUserSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository : IRepository<UserSessionUserWorkProfileSelectedUserRoleCampusSelected>
    {
        Task<UserSessionUserWorkProfileSelectedUserRoleCampusSelected?> GetByUserSessionIdAsync(int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}