using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class BlacklistedAccessTokenTemporaryRepository(ILogger<BlacklistedAccessTokenTemporaryRepository> logger, AppDbContext context) : Repository<BlacklistedAccessTokenTemporary, int>(logger, context), 
        IBlacklistedAccessTokenTemporaryRepository
    {
        public async Task<bool> ExistsAsync(string jti, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: ExistsAsync(jti: {jti})", jti);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var exists = await query.AnyAsync(batt => batt.Jti == jti, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: ExistsAsync(jti: {jti})", jti);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: ExistsAsync(jti: {jti})", jti);
                throw new PersistenceException($"Consulta terminada con error: ExistsAsync(jti: {jti})", ex);
            }
        }

        public async Task<int> PurgeExpiredBlacklistedAccessTokensTemporaryAsync(int batchSize, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0)
                batchSize = 1000;
            var totalDeleted = 0;
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            while (!cancellationToken.IsCancellationRequested)
            {
                var affected = await query.Where(t => t.ExpirationDate < utcNow).OrderBy(t => t.Id).Take(batchSize).ExecuteDeleteAsync(cancellationToken);
                if (affected <= 0)
                    break;
                totalDeleted += affected;
                if (affected < batchSize)
                    break;
            }
            _logger.LogInformation("PurgeExpiredBlacklistedAccessTokensTemporaryAsync deleted {TotalDeleted} rows.", totalDeleted);
            return totalDeleted;
        }
    }
}