using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Hub.Domain
{
    public interface IDatabaseContext
    {
        DbSet<T> Set<T>() where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        EntityEntry<T> Entry<T>(T entity) where T : class;
        void SetQueryTrackingBehavior(QueryTrackingBehavior behavior);
    }

    public class EfDatabaseContext : IDatabaseContext
    {
        private readonly DatabaseContext _context;

        public EfDatabaseContext(DatabaseContext context)
        {
            _context = context;
        }

        public DbSet<T> Set<T>() where T : class => _context.Set<T>();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);

        public EntityEntry<T> Entry<T>(T entity) where T : class => _context.Entry(entity);

        public void SetQueryTrackingBehavior(QueryTrackingBehavior behavior)
        {
            _context.ChangeTracker.QueryTrackingBehavior = behavior;
        }
    }

    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
    }
}
