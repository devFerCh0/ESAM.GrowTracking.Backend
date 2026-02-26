using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using System.Data;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IBlacklistedAccessTokenPermanentRepository BlacklistedAccessTokensPermanent { get; }

        IBlacklistedAccessTokenTemporaryRepository BlacklistedAccessTokensTemporary { get; }

        IBlacklistedRefreshTokenRepository BlacklistedRefreshTokens { get; }

        IRolePermissionRepository RolePermissions { get; }

        IUserRepository Users { get; }

        IUserDeviceRepository UserDevices { get; }

        IUserRoleCampusRepository UserRoleCampuses { get; }

        IUserSessionRepository UserSessions { get; }

        IUserSessionRefreshTokenRepository UserSessionRefreshTokens { get; }

        IUserSessionUserWorkProfileSelectedRepository UserSessionUserWorkProfilesSelected { get; }

        IUserSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository UserSessionUserWorkProfileSelectedUserRoleCampusesSelected { get; }

        IUserWorkProfileRepository UserWorkProfiles { get; }

        IWorkProfileRepository WorkProfiles { get; }

        IWorkProfilePermissionRepository WorkProfilePermissions { get; }

        Task BeginTransactionAsync(IsolationLevel? isolationLevel = null, CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}