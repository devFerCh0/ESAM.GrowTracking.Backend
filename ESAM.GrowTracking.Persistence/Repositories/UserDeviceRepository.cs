using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class UserDeviceRepository(ILogger<UserDeviceRepository> logger, AppDbContext context) : Repository<UserDevice, int>(logger, context), IUserDeviceRepository
    {
        public async Task<UserDevice?> GetByUserIdAndDeviceIdentifierAsync(int userId, string deviceIdentifier, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetByUserIdAndDeviceIdentifierAsync(userId: {userId}, deviceIdentifier: {deviceIdentifier})", userId, deviceIdentifier);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var userDevice = await query.FirstOrDefaultAsync(ud => ud.UserId == userId && ud.DeviceIdentifier == deviceIdentifier, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetByUserIdAndDeviceIdentifierAsync(userId: {userId}, deviceIdentifier: {deviceIdentifier})", userId, deviceIdentifier);
                return userDevice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetByUserIdAndDeviceIdentifierAsync(userId: {userId}, deviceIdentifier: {deviceIdentifier})", userId, deviceIdentifier);
                throw new PersistenceException($"Consulta terminada con exito: GetByUserIdAndDeviceIdentifierAsync(userId: {userId}, deviceIdentifier: {deviceIdentifier})", ex);
            }
        }

        public async Task<UserDevice?> GetByIdAndUserIdAsync(int id, int userId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetByIdAndUserIdAsync(id: {id}, userId: {userId})", id, userId);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var userDevice = await query.FirstOrDefaultAsync(ud => ud.Id == id && ud.UserId == userId, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetByIdAndUserIdAsync(id: {id}, userId: {userId})", id, userId);
                return userDevice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetByIdAndUserIdAsync(id: {id}, userId: {userId})", id, userId);
                throw new PersistenceException($"Consulta terminada con exito: GetByIdAndUserIdAsync(id: {id}, userId: {userId})", ex);
            }
        }
    }
}