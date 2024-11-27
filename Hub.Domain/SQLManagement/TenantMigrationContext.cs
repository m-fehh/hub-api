using Hub.Domain.Entities.Tenants;
using Hub.Shared.DataConfiguration;
using Microsoft.EntityFrameworkCore;

namespace Hub.Domain.Entities.SQLManagement
{
    public class TenantMigrationContext : DbContext
    {
        private readonly string _schema;

        public TenantMigrationContext(NhConfigurationData tenantConfiguration, DbContextOptions<TenantMigrationContext> options) : base(options)
        {
            _schema = tenantConfiguration.SchemaDefault ?? throw new ArgumentNullException(nameof(tenantConfiguration.SchemaDefault));
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(_schema);
        }

        public override int SaveChanges()
        {
            EnsureSchemaExists();
            return base.SaveChanges();
        }

        private void EnsureSchemaExists()
        {
            var schemaExistsQuery = $"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{_schema}') BEGIN EXEC('CREATE SCHEMA {_schema}'); END";
            Database.ExecuteSqlRaw(schemaExistsQuery);
        }
    }

}
