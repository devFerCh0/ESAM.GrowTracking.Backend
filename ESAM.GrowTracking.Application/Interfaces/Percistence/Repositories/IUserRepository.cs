using ESAM.GrowTracking.Application.Features.Auth.Login.Projections;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.Projections;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.Projections;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IUserRepository : IRepository<User, int>
    {
        Task<User?> GetByCredentialAsync(string credential, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<LoginUserProjection?> GetLoginUserByIdAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<LoginAssumedRoleUserProjection?> GetLoginAssumedRoleUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<LoginAssumedWorkProfileUserProjection?> GetLoginAssumedWorkProfileUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}