using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Hub.Domain.Database.Services
{
    public class DatabaseMigrationService
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseMigrationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Método para rodar as migrações
        public void MigrateDatabase()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }
        }
    }
}
