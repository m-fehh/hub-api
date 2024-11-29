using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Hub.Infrastructure.Database.NhManagement;

namespace Hub.Domain.Database.Runner
{
    public class DbMigrator
    {
        private readonly IDatabaseInformation _databaseInformation;

        public DbMigrator(IDatabaseInformation databaseInformation)
        {
            _databaseInformation = databaseInformation;
        }

        public void MigrateUp(Type type)
        {
            MigrateUp(type, TimeSpan.Zero);
        }

        public void MigrateUp(Type type, TimeSpan timeSpan)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .AddFluentMigratorCore().AddScoped(typeof(IVersionTableMetaData), typeof(VersionTable))
                .ConfigureRunner(builder => GetMigrationRunnerBuilder(builder, type, timeSpan))
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }
        }

        private IMigrationRunnerBuilder GetMigrationRunnerBuilder(IMigrationRunnerBuilder builder, Type type, TimeSpan timeSpan)
        {
            if (_databaseInformation.DatabaseSupplier() == "sqlserver")
            {
                builder = builder.AddSqlServer2008();
            }

            if (timeSpan != TimeSpan.Zero)
            {
                builder = builder.ConfigureGlobalProcessorOptions(option => { option.PreviewOnly = false; option.Timeout = timeSpan; });
            }

            return builder.WithGlobalConnectionString(_databaseInformation.ConnectionString()).ScanIn(type.Assembly).For.Migrations();
        }
    }
}
