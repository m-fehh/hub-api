using Autofac;
using Hub.Domain.Database.Runner;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Autofac.Dependency;
using Hub.Infrastructure.Database.NhManagement.Migrations;
using Hub.Infrastructure.Localization;

namespace Hub.API.Configuration
{
    public class DependencyRegistrar : IDependencySetup
    {
        public void Register(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultLocalizationProvider>().As<ILocalizationProvider>().AsSelf();
            builder.RegisterType<HubProvider>().AsSelf().SingleInstance();

            builder.RegisterType<DbMigrator>().AsSelf();
            builder.RegisterType<MigrationRunner>().As<IMigrationRunner>();
            builder.RegisterType<VersionManager>().As<IVersionManager>();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
