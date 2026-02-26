using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class UserSessionRefreshTokenRepository(ILogger<UserSessionRefreshTokenRepository> logger, AppDbContext context) : Repository<UserSessionRefreshToken, int>(logger, context), IUserSessionRefreshTokenRepository
    {
        public async Task<UserSessionRefreshToken?> GetByTokenIdentifierAsync(string tokenIdentifier, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetByTokenIdentifierAsync(tokenIdentifier: {tokenIdentifier})", tokenIdentifier);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var userSessionToken = await query.FirstOrDefaultAsync(ust => ust.TokenIdentifier == tokenIdentifier, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetByTokenIdentifierAsync(tokenIdentifier: {tokenIdentifier})", tokenIdentifier);
                return userSessionToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetByTokenIdentifierAsync(tokenIdentifier: {tokenIdentifier})", tokenIdentifier);
                throw new PersistenceException($"Consulta terminada con exito: GetByTokenIdentifierAsync(tokenIdentifier: {tokenIdentifier})", ex);
            }
        }

        public async Task<List<UserSessionRefreshToken>> GetAllByUserSessionIdAsync(int userSessionId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetAllByUserSessionIdAsync(userSessionId: {userSessionId})", userSessionId);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var userSessionTokens = await query.Where(ust => ust.UserSessionId == userSessionId).ToListAsync(cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetAllByUserSessionIdAsync(userSessionId: {userSessionId})", userSessionId);
                return userSessionTokens;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetAllByUserSessionIdAsync(userSessionId: {userSessionId})", userSessionId);
                throw new PersistenceException($"Consulta terminada con exito: GetAllByUserSessionIdAsync(userSessionId: {userSessionId})", ex);
            }
        }

        public async Task<int> PurgeExpiredUserSessionRefreshTokensAsync(int batchSize, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0) batchSize = 1000;
            var totalDeleted = 0;
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            while (!cancellationToken.IsCancellationRequested)
            {
                var affected = await query.Where(t => t.ExpiresAt < utcNow).OrderBy(t => t.Id).Take(batchSize).ExecuteDeleteAsync(cancellationToken);
                if (affected <= 0)
                    break;
                totalDeleted += affected;
                if (affected < batchSize)
                    break;
            }
            _logger.LogInformation("PurgeExpiredUserSessionRefreshTokensAsync deleted {TotalDeleted} rows.", totalDeleted);
            return totalDeleted;
        }
    }
}