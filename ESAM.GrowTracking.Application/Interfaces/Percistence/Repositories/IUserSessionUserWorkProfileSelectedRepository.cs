using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IUserSessionUserWorkProfileSelectedRepository : IRepository<UserSessionUserWorkProfileSelected>
    {
        Task<UserSessionUserWorkProfileSelected?> GetByUserSessionIdAsync(int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}