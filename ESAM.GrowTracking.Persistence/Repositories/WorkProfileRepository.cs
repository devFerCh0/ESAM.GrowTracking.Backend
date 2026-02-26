using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Catalogs;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class WorkProfileRepository(ILogger<WorkProfileRepository> logger, AppDbContext context) : Repository<WorkProfile, int>(logger, context), IWorkProfileRepository
    {
        public async Task<bool> IsValidWorkProfileTypeAsync(int id, WorkProfileType workProfileType, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: IsValidWorkProfileTypeAsync(id: {id}, workProfileType: {workProfileType})", id, workProfileType);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var isValidWorkProfileType = await query.AnyAsync(wp => wp.Id == id && wp.WorkProfileType == workProfileType && !wp.IsDeleted, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: IsValidWorkProfileTypeAsync(id: {id}, workProfileType: {workProfileType})", id, workProfileType);
                return isValidWorkProfileType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: IsValidWorkProfileTypeAsync(id: {id}, workProfileType: {workProfileType})", id, workProfileType);
                throw new PersistenceException($"Consulta terminada con error: IsValidWorkProfileTypeAsync(id: {id}, workProfileType: {workProfileType})", ex);
            }
        }

        public async Task<WorkProfileType> GetWorkProfileTypeById(int id, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetWorkProfileTypeById(id: {id})", id);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var workProfileType = await query.Where(wp => wp.Id == id).Select(wp => wp.WorkProfileType).FirstOrDefaultAsync(cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetWorkProfileTypeById(id: {id})", id);
                return workProfileType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetWorkProfileTypeById(id: {id})", id);
                throw new PersistenceException($"Consulta terminada con error: GetWorkProfileTypeById(id: {id})", ex);
            }
        }
    }
}