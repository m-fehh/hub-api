using Hub.Domain.Entities.Admin;
using Microsoft.EntityFrameworkCore;

namespace Hub.Domain.SQLManagement
{
    public class AdminContext : DbContext
    {
        public DbSet<Tenant> Tenants { get; set; } = null!;

        public AdminContext(DbContextOptions<AdminContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("adm");
        }

        public override int SaveChanges()
        {
            EnsureSchemaExists();
            return base.SaveChanges();
        }

        private void EnsureSchemaExists()
        {
            var schemaExistsQuery = "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'adm') BEGIN EXEC('CREATE SCHEMA adm'); END";
            Database.ExecuteSqlRaw(schemaExistsQuery);
        }
    }
}
