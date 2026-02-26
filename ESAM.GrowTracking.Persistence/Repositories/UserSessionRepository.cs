using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class UserSessionRepository(ILogger<UserSessionRepository> logger, AppDbContext context) : Repository<UserSession, int>(logger, context), IUserSessionRepository
    {
        public async Task<UserSession?> GetByIdAndUserIdAnduserDeviceIdAsync(int id, int userId, int userDeviceId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetByIdAndUserIdAsync(id: {id}, userId: {userId}, userDeviceId: {userDeviceId})", id, userId, userDeviceId);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var userSession = await query.FirstOrDefaultAsync(us => us.Id == id && us.UserId == userId && us.UserDeviceId == userDeviceId, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetByIdAndUserIdAsync(id: {id}, userId: {userId}, userDeviceId: {userDeviceId})", id, userId, userDeviceId);
                return userSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetByIdAndUserIdAsync(id: {id}, userId: {userId}, userDeviceId: {userDeviceId})", id, userId, userDeviceId);
                throw new PersistenceException($"Consulta terminada con exito: GetByIdAndUserIdAsync(id: {id}, userId: {userId}), userDeviceId: {userDeviceId}", ex);
            }
        }
    }
}