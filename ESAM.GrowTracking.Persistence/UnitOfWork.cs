using ESAM.GrowTracking.Application.Interfaces.Percistence;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ESAM.GrowTracking.Persistence
{
    public class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
    {
        private readonly ILogger<UnitOfWork> _logger;
        private readonly AppDbContext _context;
        private readonly IBlacklistedAccessTokenPermanentRepository _blacklistedAccessTokensPermanent;
        private readonly IBlacklistedAccessTokenTemporaryRepository _blacklistedAccessTokensTemporary;
        private readonly IBlacklistedRefreshTokenRepository _blacklistedRefreshTokens;
        private readonly IRolePermissionRepository _rolePermissions;
        private readonly IUserRepository _users;
        private readonly IUserDeviceRepository _userDevices;
        private readonly IUserRoleCampusRepository _userRoleCampuses;
        private readonly IUserSessionRepository _userSessions;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokens;
        private readonly IUserSessionUserWorkProfileSelectedRepository _userSessionUserWorkProfilesSelected;
        private readonly IUserSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository _userSessionUserWorkProfileSelectedUserRoleCampusesSelected;
        private readonly IUserWorkProfileRepository _userWorkProfiles;
        private readonly IWorkProfileRepository _workProfiles;
        private readonly IWorkProfilePermissionRepository _workProfilePermissions;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        public UnitOfWork(ILogger<UnitOfWork> logger, AppDbContext context, IBlacklistedAccessTokenPermanentRepository blacklistedAccessTokensPermanent, 
            IBlacklistedAccessTokenTemporaryRepository blacklistedAccessTokensTemporary, IBlacklistedRefreshTokenRepository blacklistedRefreshTokens, IRolePermissionRepository rolePermissions, IUserRepository users, 
            IUserDeviceRepository userDevices, IUserRoleCampusRepository userRoleCampuses, IUserSessionRepository userSessions, IUserSessionRefreshTokenRepository userSessionRefreshTokens,
            IUserSessionUserWorkProfileSelectedRepository userSessionUserWorkProfilesSelected, IUserSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository userSessionUserWorkProfileSelectedUserRoleCampusesSelected, 
            IUserWorkProfileRepository userWorkProfiles, IWorkProfileRepository workProfiles, IWorkProfilePermissionRepository workProfilePermissions)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(context, $"{nameof(context)} no puede ser nulo.");
            Guard.AgainstNull(blacklistedAccessTokensPermanent, $"{nameof(blacklistedAccessTokensPermanent)} no puede ser nulo.");
            Guard.AgainstNull(blacklistedAccessTokensTemporary, $"{nameof(blacklistedAccessTokensTemporary)} no puede ser nulo.");
            Guard.AgainstNull(blacklistedRefreshTokens, $"{nameof(blacklistedRefreshTokens)} no puede ser nulo.");
            Guard.AgainstNull(rolePermissions, $"{nameof(rolePermissions)} no puede ser nulo.");
            Guard.AgainstNull(users, $"{nameof(users)} no puede ser nulo.");
            Guard.AgainstNull(userDevices, $"{nameof(userDevices)} no puede ser nulo.");
            Guard.AgainstNull(userRoleCampuses, $"{nameof(userRoleCampuses)} no puede ser nulo.");
            Guard.AgainstNull(userSessions, $"{nameof(userSessions)} no puede ser nulo.");
            Guard.AgainstNull(userSessionRefreshTokens, $"{nameof(userSessionRefreshTokens)} no puede ser nulo.");
            Guard.AgainstNull(userSessionUserWorkProfilesSelected, $"{nameof(userSessionUserWorkProfilesSelected)} no puede ser nulo.");
            Guard.AgainstNull(userSessionUserWorkProfileSelectedUserRoleCampusesSelected, $"{nameof(userSessionUserWorkProfileSelectedUserRoleCampusesSelected)} no puede ser nulo.");
            Guard.AgainstNull(userWorkProfiles, $"{nameof(userWorkProfiles)} no puede ser nulo.");
            Guard.AgainstNull(workProfiles, $"{nameof(workProfiles)} no puede ser nulo.");
            Guard.AgainstNull(workProfilePermissions, $"{nameof(workProfilePermissions)} no puede ser nulo.");
            _logger = logger;
            _context = context;
            _blacklistedAccessTokensPermanent = blacklistedAccessTokensPermanent;
            _blacklistedAccessTokensTemporary = blacklistedAccessTokensTemporary;
            _blacklistedRefreshTokens = blacklistedRefreshTokens;
            _rolePermissions = rolePermissions;
            _users = users;
            _userDevices = userDevices;
            _userRoleCampuses = userRoleCampuses;
            _userSessions = userSessions;
            _userSessionRefreshTokens = userSessionRefreshTokens;
            _userSessionUserWorkProfilesSelected = userSessionUserWorkProfilesSelected;
            _userSessionUserWorkProfileSelectedUserRoleCampusesSelected = userSessionUserWorkProfileSelectedUserRoleCampusesSelected;
            _userWorkProfiles = userWorkProfiles;
            _workProfiles = workProfiles;
            _workProfilePermissions = workProfilePermissions;
        }

        public IBlacklistedAccessTokenPermanentRepository BlacklistedAccessTokensPermanent => _blacklistedAccessTokensPermanent;

        public IBlacklistedAccessTokenTemporaryRepository BlacklistedAccessTokensTemporary => _blacklistedAccessTokensTemporary;

        public IBlacklistedRefreshTokenRepository BlacklistedRefreshTokens => _blacklistedRefreshTokens;

        public IRolePermissionRepository RolePermissions => _rolePermissions;

        public IUserRepository Users => _users;

        public IUserDeviceRepository UserDevices => _userDevices;

        public IUserRoleCampusRepository UserRoleCampuses => _userRoleCampuses;

        public IUserSessionRepository UserSessions => _userSessions;

        public IUserSessionRefreshTokenRepository UserSessionRefreshTokens => _userSessionRefreshTokens;

        public IUserSessionUserWorkProfileSelectedRepository UserSessionUserWorkProfilesSelected => _userSessionUserWorkProfilesSelected;

        public IUserSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository UserSessionUserWorkProfileSelectedUserRoleCampusesSelected => _userSessionUserWorkProfileSelectedUserRoleCampusesSelected;

        public IUserWorkProfileRepository UserWorkProfiles => _userWorkProfiles;

        public IWorkProfileRepository WorkProfiles => _workProfiles;

        public IWorkProfilePermissionRepository WorkProfilePermissions => _workProfilePermissions;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            _logger.LogDebug("Guardando cambios...");
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios en la base de datos.");
                throw new PersistenceException("Error al guardar cambios en la base de datos.", ex);
            }
        }

        public async Task BeginTransactionAsync(IsolationLevel? isolationLevel = null, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_transaction is not null)
                {
                    _logger.LogWarning("Transacción ya en curso.");
                    return;
                }
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    _transaction = isolationLevel.HasValue
                        ? await _context.Database.BeginTransactionAsync(isolationLevel.Value, cancellationToken)
                        : await _context.Database.BeginTransactionAsync(cancellationToken);
                    _logger.LogInformation("Transacción iniciada (Nivel de aislamiento={Level})", _transaction.GetDbTransaction().IsolationLevel);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo iniciar la transacción.");
                throw new PersistenceException("No se pudo iniciar la transacción.", ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_transaction is null)
                {
                    _logger.LogWarning("No hay transacción activa para confirmar.");
                    return;
                }
                try
                {
                    await SaveChangesAsync(cancellationToken);
                    await _transaction.CommitAsync(cancellationToken);
                    _logger.LogInformation("Transacción confirmada.");
                }
                catch (Exception commitEx)
                {
                    _logger.LogError(commitEx, "Fallo al confirmar; intentando deshacer.");
                    try
                    {
                        await _transaction.RollbackAsync(cancellationToken);
                        _logger.LogInformation("Rollback exitoso tras fallo de confirmación.");
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "Rollback fallido tras fallo de confirmación.");
                        throw new PersistenceException("Error durante rollback tras fallo de commit.", new AggregateException(commitEx, rollbackEx));
                    }
                    throw new PersistenceException("Error al confirmar transacción.", commitEx);
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_transaction is null)
                {
                    _logger.LogWarning("No hay transacción activa para deshacer.");
                    return;
                }
                try
                {
                    await _transaction.RollbackAsync(cancellationToken);
                    _logger.LogInformation("Transacción revertida.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error durante el rollback.");
                    throw new PersistenceException("Error al deshacer la transacción.", ex);
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(action, $"{nameof(action)} no puede ser nulo.");
            EnsureNotDisposed();
            await BeginTransactionAsync(cancellationToken: cancellationToken);
            try
            {
                await action(cancellationToken);
                await CommitTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la acción; realizando rollback");
                await RollbackTransactionAsync(cancellationToken);
                throw new PersistenceException("Ocurrió un error durante la ejecución de la acción en la transacción.", ex);
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new PersistenceException("No puede usarse después de haber sido liberado.");
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _transaction?.Dispose();
            _context.Dispose();
            _semaphore.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;
            if (_transaction != null)
                await _transaction.DisposeAsync();
            await _context.DisposeAsync();
            _semaphore.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}