using ESAM.GrowTracking.Application.Interfaces.Percistence;
using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly ILogger<Repository<TEntity>> _logger;
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(ILogger<Repository<TEntity>> logger, AppDbContext context)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(context, $"{nameof(context)} no puede ser nulo.");
            _logger = logger;
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public virtual async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(entity, $"{nameof(entity)} no puede ser nulo.");
            await _dbSet.AddAsync(entity, cancellationToken);
            _logger.LogDebug("InsertAsync<{Entity}>: entidad preparada para insertar", typeof(TEntity).Name);
        }
    }

    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : notnull
    {
        protected readonly ILogger<Repository<TEntity, TKey>> _logger;
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(ILogger<Repository<TEntity, TKey>> logger, AppDbContext context)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(context, $"{nameof(context)} no puede ser nulo.");
            _logger = logger;
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(TKey id, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(id, $"{nameof(id)} no puede ser nulo.");
            _logger.LogDebug("GetByIdAsync<{Entity}>({Id}) started", typeof(TEntity).Name, id);
            var query = asTracking ? _dbSet : _dbSet.AsNoTracking();
            var entity = await query.FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
            _logger.LogDebug("GetByIdAsync<{Entity}> completed", typeof(TEntity).Name);
            return entity;
        }

        public virtual async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(entity, $"{nameof(entity)} no puede ser nulo.");
            await _dbSet.AddAsync(entity, cancellationToken);
            _logger.LogDebug("InsertAsync<{Entity}>: entidad preparada para insertar con Id={Id}", typeof(TEntity).Name, entity.Id);
        }

        public virtual async Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(entities, $"{nameof(entities)} no puede ser nulo.");
            _logger.LogDebug("InsertRangeAsync<{Entity}> queued entities", typeof(TEntity).Name);
            var list = entities as IList<TEntity> ?? [.. entities];
            var originalDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            try
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;
                await _dbSet.AddRangeAsync(list, cancellationToken);
                _logger.LogDebug("Queued {Count} items", list.Count);
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalDetect;
            }
        }

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(entity, $"{nameof(entity)} no puede ser nulo.");
            _dbSet.Update(entity);
            _logger.LogDebug("Update<{Entity}>: entidad preparada para actualizar con Id={Id}", typeof(TEntity).Name, entity.Id);
            await Task.CompletedTask;
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(entities, $"{nameof(entities)} no puede ser nulo.");
            _logger.LogDebug("UpdateRange<{Entity}> queued entities", typeof(TEntity).Name);
            var list = entities as IList<TEntity> ?? [.. entities];
            var originalDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            try
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;
                _dbSet.UpdateRange(list);
                _logger.LogDebug("Queued {Count} items", list.Count);
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalDetect;
            }
            await Task.CompletedTask;
        }
    }
}