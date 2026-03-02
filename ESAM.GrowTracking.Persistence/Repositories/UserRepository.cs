using ESAM.GrowTracking.Application.Features.Auth.Login.Projections;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.Projections;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.Projections;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class UserRepository(ILogger<UserRepository> logger, AppDbContext context) : Repository<User, int>(logger, context), IUserRepository
    {
        public async Task<User?> GetByCredentialAsync(string credential, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetByCredentialAsync(credential: {credential})", credential);
            var normalizedCredential = credential.Trim().ToUpperInvariant();
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var user = await query.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedCredential || u.NormalizedEmail == normalizedCredential, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetByCredentialAsync(credential: {credential})", credential);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetByCredentialAsync(credential: {credential})", credential);
                throw new PersistenceException($"Consulta terminada con error: GetByCredentialAsync(credential: {credential})", ex);
            }
        }


        public async Task<bool> ValidateUserStatusAsync(int id, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: ValidateUserStatusAsync(id: {id}, utcNow: {utcNow})", id, utcNow);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var validateUserStatus = await query.AnyAsync(u => u.Id == id && !u.IsDeleted && (u.LockoutEndAt == null || u.LockoutEndAt.Value <= utcNow), cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: ValidateUserStatusAsync(id: {id}, utcNow: {utcNow})", id, utcNow);
                return validateUserStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: ValidateUserStatusAsync(id: {id}, utcNow: {utcNow})", id, utcNow);
                throw new PersistenceException($"Consulta terminada con error: ValidateUserStatusAsync(id: {id}, utcNow: {utcNow})", ex);
            }
        }

        public async Task<bool> ValidateUserSecurityAsync(int id, string securityStamp, int tokenVersion, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: ValidateUserSecurityAsync(id: {id}, securityStamp: {securityStamp}, tokenVersion: {tokenVersion})", id, securityStamp, tokenVersion);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var validateUserSecurity = await query.AnyAsync(u => u.Id == id && u.SecurityStamp == securityStamp && u.TokenVersion == tokenVersion, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: ValidateUserSecurityAsync(id: {id}, securityStamp: {securityStamp}, tokenVersion: {tokenVersion})", id, securityStamp, tokenVersion);
                return validateUserSecurity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: ValidateUserSecurityAsync(id: {id}, securityStamp: {securityStamp}, tokenVersion: {tokenVersion})", id, securityStamp, tokenVersion);
                throw new PersistenceException($"Consulta terminada con error: ValidateUserSecurityAsync(id: {id}, securityStamp: {securityStamp}, tokenVersion: {tokenVersion})", ex);
            }
        }

        public async Task<LoginUserProjection?> GetLoginUserByIdAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetLoginUserByIdAsync(id: {id})", id);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var loginUser = await query.Where(u => u.Id == id && !u.IsDeleted)
                    .Select(u => new LoginUserProjection(u.Id, u.Username, u.Email, u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                        u.UserWorkProfiles.Where(up => !up.IsDeleted).Select(uwp => new LoginUserWorkProfileProjection(uwp.WorkProfileId, uwp.WorkProfile.Name, uwp.WorkProfile.WorkProfileType)).ToList()))
                    .FirstOrDefaultAsync(cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetLoginUserByIdAsync(id: {id})", id);
                return loginUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetLoginUserByIdAsync(id: {id})", id);
                throw new PersistenceException($"Consulta terminada con error: GetLoginUserByIdAsync(id: {id})", ex);
            }
        }

        public async Task<LoginAssumedRoleUserProjection?> GetLoginAssumedRoleUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetLoginAssumedRoleUserByUserIdAndUserSessionIdAsync(userId: {userId}, userSessionId: {userSessionId})", userId, userSessionId);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var loginAssumedRoleUser = await query.Where(u => u.Id == userId)
                    .Select(u => new LoginAssumedRoleUserProjection(
                        u.Id, u.Username, u.Email, u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                        u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                        u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new LoginAssumedRoleUserWorkProfileProjection(uwp.WorkProfileId, uwp.WorkProfile.Name, uwp.WorkProfile.WorkProfileType)).ToList(),
                        u.UserRoleCampuses.Where(urc => !urc.IsDeleted).Select(urc => new LoginAssumedRoleUserRoleCampusProjection(urc.RoleId, urc.Role.Name, urc.CampusId, urc.Campus.Name)).ToList(),
                        u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new LoginAssumedRoleUserSessionProjection(us.Id, us.IpAddress, us.UserAgent,
                            us.UserSessionUserWorkProfileSelected != null ? new LoginAssumedRoleUserSessionUserWorkProfileSelectedProjection(us.UserSessionUserWorkProfileSelected.WorkProfileId, 
                                us.UserSessionUserWorkProfileSelected.UserSessionUserWorkProfileSelectedUserRoleCampusSelected != null ? new LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedProjection(
                                    us.UserSessionUserWorkProfileSelected.UserSessionUserWorkProfileSelectedUserRoleCampusSelected.RoleId, 
                                    us.UserSessionUserWorkProfileSelected.UserSessionUserWorkProfileSelectedUserRoleCampusSelected.CampusId) : null) : null)).FirstOrDefault())).FirstOrDefaultAsync(cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetLoginAssumedRoleUserByUserIdAndUserSessionIdAsync(userId: {userId}, userSessionId: {userSessionId})", userId, userSessionId);
                return loginAssumedRoleUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetLoginAssumedRoleUserByUserIdAndUserSessionIdAsync(userId: {userId}, userSessionId: {userSessionId}))", userId, userSessionId);
                throw new PersistenceException($"Consulta terminada con error: GetLoginAssumedRoleUserByUserIdAndUserSessionIdAsync(userId: {userId}, userSessionId: {userSessionId}))", ex);
            }
        }

        public async Task<LoginAssumedWorkProfileUserProjection?> GetLoginAssumedWorkProfileUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetLoginAssumedWorkProfileUserByUserIdAndUserSessionIdAsync(userId: {userId}, userSessionId: {userSessionId})", userId, userSessionId);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var loginAssumedWorkProfileUser = await query.Where(u => u.Id == userId)
                    .Select(u => new LoginAssumedWorkProfileUserProjection(
                        u.Id, u.Username, u.Email, u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                        u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                        u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new LoginAssumedWorkProfileUserWorkProfileProjection(uwp.WorkProfileId, uwp.WorkProfile.Name, uwp.WorkProfile.WorkProfileType)).ToList(),
                        u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new LoginAssumedWorkProfileUserSessionProjection(us.Id, us.IpAddress, us.UserAgent, 
                            us.UserSessionUserWorkProfileSelected != null ? new LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedProjection(us.UserSessionUserWorkProfileSelected.WorkProfileId) : null)).FirstOrDefault()))
                    .FirstOrDefaultAsync(cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetLoginAssumedWorkProfileUserByUserIdAndUserSessionIdAsync(userId: {userId}, userSessionId: {userSessionId})", userId, userSessionId);
                return loginAssumedWorkProfileUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetLoginAssumedWorkProfileUserByUserIdAndUserSessionIdAsync(userId: {userId}, userSessionId: {userSessionId}))", userId, userSessionId);
                throw new PersistenceException($"Consulta terminada con error: GetLoginAssumedWorkProfileUserByUserIdAndUserSessionIdAsync(userId: {userId}, userSessionId: {userSessionId}))", ex);
            }
        }
    }
}