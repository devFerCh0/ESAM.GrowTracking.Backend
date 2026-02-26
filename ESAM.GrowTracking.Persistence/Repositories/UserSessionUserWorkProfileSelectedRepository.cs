using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class UserSessionUserWorkProfileSelectedRepository(ILogger<UserSessionUserWorkProfileSelectedRepository> logger, AppDbContext context) : Repository<UserSessionUserWorkProfileSelected>(logger, context),
        IUserSessionUserWorkProfileSelectedRepository
    {
        public async Task<UserSessionUserWorkProfileSelected?> GetByUserSessionIdAsync(int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetByUserSessionIdAsync(userSessionId: {userSessionId})", userSessionId);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var userSessionUserWorkProfileSelected = await query.FirstOrDefaultAsync(usuwps => usuwps.UserSessionId == userSessionId, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetByUserSessionIdAsync(userSessionId: {userSessionId})", userSessionId);
                return userSessionUserWorkProfileSelected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetByUserSessionIdAsync(userSessionId: {userSessionId})", userSessionId);
                throw new PersistenceException($"Consulta terminada con exito: GetByUserSessionIdAsync(userSessionId: {userSessionId})", ex);
            }
        }
    }
}