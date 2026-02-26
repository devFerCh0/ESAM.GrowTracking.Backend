using ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class UserRoleCampusRepository(ILogger<UserRoleCampusRepository> logger, AppDbContext context): Repository<UserRoleCampus>(logger, context), IUserRoleCampusRepository
    {
        public async Task<List<LoginUserRoleCampusProjection>> GetLoginUserRoleCampusesByUserIdAsync(int userId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetLoginUserRoleCampusesByUserIdAsync(userId: {userId})", userId);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var userRoleCampuses = await query.Where(urc => urc.UserId == userId && !urc.IsDeleted && urc.Role.RolePermissions.Any(rp => rp.HasAccess && !rp.Permission.IsDeleted))
                    .Select(urc => new LoginUserRoleCampusProjection(urc.RoleId, urc.Role.Name, urc.CampusId, urc.Campus.Name)).ToListAsync(cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetLoginUserRoleCampusesByUserIdAsync(userId: {userId})", userId);
                return userRoleCampuses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetLoginUserRoleCampusesByUserIdAsync(userId: {userId})", userId);
                throw new PersistenceException($"Consulta terminada con error: GetLoginUserRoleCampusesByUserIdAsync(userId: {userId})", ex);
            }
        }

        public async Task<UserRoleCampus?> GetByUserIdAndRoleIdAndCampusIdAsync(int userId, int roleId, int campusId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetByUserIdAndRoleIdAndCampusIdAsync(userId: {userId}, roleId: {roleId}, campusId: {campusId})", userId, roleId, campusId);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var userRoleCampus = await query.FirstOrDefaultAsync(urc => urc.UserId == userId && urc.RoleId == roleId && urc.CampusId == campusId, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetByUserIdAndRoleIdAndCampusIdAsync(userId: {userId}, roleId: {roleId}, campusId: {campusId})", userId, roleId, campusId);
                return userRoleCampus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetByUserIdAndRoleIdAndCampusIdAsync(userId: {userId}, roleId: {roleId}, campusId: {campusId})", userId, roleId, campusId);
                throw new PersistenceException($"Consulta terminada con exito: GetByUserIdAndRoleIdAndCampusIdAsync(userId: {userId}, roleId: {roleId}, campusId: {campusId})", ex);
            }
        }
    }
}