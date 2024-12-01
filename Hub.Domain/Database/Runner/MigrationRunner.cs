using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure;

using Hub.Infrastructure.Database.NhManagement.Migrations;
using Hub.Infrastructure.Redis;
using Hub.Infrastructure.Lock;

namespace Hub.Domain.Database.Runner
{
    public class MigrationRunner : IMigrationRunner
    {
        private readonly DbMigrator _dbMigrator;

        public MigrationRunner(DbMigrator dbMigrator)
        {
            _dbMigrator = dbMigrator;
        }

        /// <summary>
        /// Efetua a migração do banco de dados para a última versão utilizando a API FluentMigrator
        /// </summary>
        public void MigrateToLatest()
        {
            var redisService = Engine.Resolve<IRedisService>();

            var currentAppVersion = Engine.Resolve<IVersionManager>().GetVersion();

            if (redisService.Get("ApplicationVersion").ToString() != currentAppVersion)
            {
                redisService.Set("ApplicationVersion", currentAppVersion);

                var lockName = "InitSessionFactory:" + Singleton<ISchemaNameProvider>.Instance.TenantName().FirstCharToUpper();

                using (Engine.Resolve<RedLockManager>().Lock(lockName))
                {
                    try
                    {
                        _dbMigrator.MigrateUp(typeof(MigrationRunner), TimeSpan.FromSeconds(3600));

                        ForeignKeyMap.Map(null);

                        StartupDataManager.ExecuteAll();
                    }
                    catch (Exception ex)
                    {
                        redisService.Set("ApplicationVersion", null);
                        log4net.LogManager.GetLogger("Sch.SystemInfo").Error("MIGRATION ERROR", ex);

                        throw new BusinessException(ex.CreateExceptionString());
                    }
                }
            }
            else
            {
                ForeignKeyMap.Map(null);
            }
        }
    }
}
