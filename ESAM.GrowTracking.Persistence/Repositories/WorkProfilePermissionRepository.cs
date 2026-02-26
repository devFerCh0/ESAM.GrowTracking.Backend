using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class WorkProfilePermissionRepository(ILogger<WorkProfilePermissionRepository> logger, AppDbContext context) : Repository<WorkProfilePermission>(logger, context), IWorkProfilePermissionRepository
    {
        public async Task<bool> HasActivePermissionsAsync(int workProfileId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: HasActivePermissionsAsync(workProfileId: {workProfileId})", workProfileId);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var hasActivePermissions = await query.Where(wpp => wpp.HasAccess).AnyAsync(wpp => wpp.WorkProfileId == workProfileId, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: HasActivePermissionsAsync(workProfileId: {workProfileId})", workProfileId);
                return hasActivePermissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: HasActivePermissionsAsync(workProfileId: {workProfileId})", workProfileId);
                throw new PersistenceException($"Consulta terminada con error: HasActivePermissionsAsync(workProfileId: {workProfileId})", ex);
            }
        }
    }
}