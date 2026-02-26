using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class RolePermissionRepository(ILogger<RolePermissionRepository> logger, AppDbContext context) : Repository<RolePermission>(logger, context), IRolePermissionRepository
    {
        public async Task<bool> HasActivePermissionsAsync(int roleId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: HasActivePermissionsAsync(roleId: {roleId})", roleId);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var hasActivePermissions = await query.Where(rp => rp.HasAccess).AnyAsync(rp => rp.RoleId == roleId, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: HasActivePermissionsAsync(roleId: {roleId})", roleId);
                return hasActivePermissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: HasActivePermissionsAsync(roleId: {roleId})", roleId);
                throw new PersistenceException($"Consulta terminada con error: HasActivePermissionsAsync(roleId: {roleId})", ex);
            }
        }
    }
}