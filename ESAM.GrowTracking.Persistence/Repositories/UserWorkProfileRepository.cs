using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Repositories
{
    public class UserWorkProfileRepository(ILogger<UserWorkProfileRepository> logger, AppDbContext context) : Repository<UserWorkProfile>(logger, context), IUserWorkProfileRepository
    {
        public async Task<UserWorkProfile?> GetByUserIdAndWorkProfileIdAsync(int userId, int workProfileId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Consulta iniciada: GetByUserIdAndWorkProfileIdAsync(userId: {userId}, workProfileId: {workProfileId})", userId, workProfileId);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            try
            {
                var userWorkProfile = await query.FirstOrDefaultAsync(uwp => uwp.UserId == userId && uwp.WorkProfileId == workProfileId, cancellationToken);
                _logger.LogDebug("Consulta terminada con exito: GetByUserIdAndWorkProfileIdAsync(userId: {userId}, workProfileId: {workProfileId})",userId, workProfileId);
                return userWorkProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consulta terminada con error: GetByUserIdAndWorkProfileIdAsync(userId: {userId}, workProfileId: {workProfileId})", userId, workProfileId);
                throw new PersistenceException($"Consulta terminada con exito: GetByUserIdAndWorkProfileIdAsync(userId: {userId}, workProfileId: {workProfileId})", ex);
            }
        }
    }
}