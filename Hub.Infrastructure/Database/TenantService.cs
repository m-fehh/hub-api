using Hub.Domain.Entities.Admin;
using Hub.Domain.Entities.SQLManagement;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Shared.DataConfiguration;
using Microsoft.EntityFrameworkCore;

namespace Hub.Infrastructure.Database
{
    public class TenantService
    {
        private readonly DbContextOptions<TenantMigrationContext> _dbContextOptions;

        public TenantService(DbContextOptions<TenantMigrationContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions ?? throw new ArgumentNullException(nameof(dbContextOptions));
        }

        public void MigrateTenants()
        {
            var tenants = Engine.Resolve<IRepository<Tenant>>().Table.ToList();

            foreach (var tenant in tenants)
            {
                ApplyTenantMigration(tenant);
            }
        }

        private void ApplyTenantMigration(Tenant tenant)
        {
            // Create a custom DbContext for the tenant with the appropriate schema
            var tenantConfig = new NhConfigurationData
            {
                SchemaDefault = tenant.Schema
            };

            var tenantDbContext = new TenantMigrationContext(tenantConfig, _dbContextOptions);

            tenantDbContext.Database.EnsureCreated();

            tenantDbContext.Database.Migrate();

            Console.WriteLine($"Migrations applied successfully for tenant: {tenant.Name} with schema: {tenant.Schema}");
        }
    }
}
