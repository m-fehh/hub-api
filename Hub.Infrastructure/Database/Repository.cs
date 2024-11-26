using Hub.Domain;
using Hub.Infrastructure.Logger.Interfaces;
using Hub.Shared.Enums.Infrastructure;
using Hub.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hub.Infrastructure.Database
{
    internal class Repository<T> : IRepository<T> where T : class, IBaseEntity
    {
        private readonly IDatabaseContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly ILogManager _logManager;

        public Repository(IDatabaseContext context, ILogManager logManager = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
            _logManager = logManager;
        }

        public async Task<T> GetByIdAsync(long id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task RefreshAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await _context.Entry(entity).ReloadAsync();
        }

        public async Task<long> InsertAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (entity is IModificationControl modEntity)
            {
                modEntity.CreationUTC = DateTime.UtcNow;
                modEntity.LastUpdateUTC = DateTime.UtcNow;
            }

            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();

            Log(entity, ELogAction.Insertion);

            return entity.Id;
        }

        public async Task UpdateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (entity is IModificationControl modEntity)
            {
                modEntity.LastUpdateUTC = DateTime.UtcNow;
            }

            _dbSet.Update(entity);
            await _context.SaveChangesAsync();

            Log(entity, ELogAction.Update);
        }

        public async Task DeleteAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();

            Log(entity, ELogAction.Deletion);
        }

        public async Task DeleteByIdAsync(long id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public IQueryable<T> Table => _dbSet.AsQueryable();

        public IQueryable<T> CacheableTable => Table;

        private void Log(T entity, ELogAction action)
        {
            if (_logManager == null) return;

            _logManager.Audit(entity, action, true, action == ELogAction.Update);
        }
    }

    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(long id);
        Task RefreshAsync(T entity);
        Task<long> InsertAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteByIdAsync(long id);
        IQueryable<T> Table { get; }
        IQueryable<T> CacheableTable { get; }
    }
}
