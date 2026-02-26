using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class BlacklistedRefreshTokenRepository(ILogger<BlacklistedRefreshTokenRepository> logger, AppDbContext context) : Repository<BlacklistedRefreshToken, int>(logger, context), IBlacklistedRefreshTokenRepository
    {
        public async Task<List<string>> GetExistingTokenIdentifiersAsync(List<string> tokenIdentifiers, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetExistingTokenIdentifiersAsync()");
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var existingTokenIdentifiers = await query.Where(brt => tokenIdentifiers.Contains(brt.TokenIdentifier)).Select(brt => brt.TokenIdentifier).ToListAsync(cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetExistingTokenIdentifiersAsync()");
                return existingTokenIdentifiers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetExistingTokenIdentifiersAsync()");
                throw new PersistenceException($"Consulta terminada con error: GetExistingTokenIdentifiersAsync()", ex);
            }
        }

        public async Task<bool> ExistsAsync(string tokenIdentifier, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: ExistsAsync(tokenIdentifier: {tokenIdentifier})", tokenIdentifier);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var exists = await query.AnyAsync(brt => brt.TokenIdentifier == tokenIdentifier, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: ExistsAsync(tokenIdentifier: {tokenIdentifier})", tokenIdentifier);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: ExistsAsync(tokenIdentifier: {tokenIdentifier})", tokenIdentifier);
                throw new PersistenceException($"Consulta terminada con error: ExistsAsync(tokenIdentifier: {tokenIdentifier})", ex);
            }
        }

        public async Task<int> PurgeExpiredBlacklistedRefreshTokensAsync(int batchSize, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0) batchSize = 1000;
            var totalDeleted = 0;
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            while (!cancellationToken.IsCancellationRequested)
            {
                var affected = await _dbSet.Where(t => t.ExpirationDate < utcNow).OrderBy(t => t.Id).Take(batchSize).ExecuteDeleteAsync(cancellationToken);
                if (affected <= 0)
                    break;
                totalDeleted += affected;
                if (affected < batchSize)
                    break;
            }
            _logger.LogInformation("PurgeExpiredBlacklistedRefreshTokensAsync deleted {TotalDeleted} rows.", totalDeleted);
            return totalDeleted;
        }
    }
}