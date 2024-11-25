using Autofac;
using Hub.Infrastructure.Database;
using Hub.Infrastructure.Localization;
using Hub.Infrastructure.MultiTenant;
using System.Collections.Generic;

namespace Hub.Infrastructure.Autofac.Dependency
{
    public class DependencySetup : IDependencySetup
    {
        public void Register(ContainerBuilder builder)
        {
            builder.RegisterType<TenantLifeTimeScope>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<DefaultTenantManager>().As<ITenantManager>().SingleInstance();
            builder.RegisterType<ConnectionStringBaseConfigurator>().AsSelf().SingleInstance();
            builder.RegisterType<DefaultLocalizationProvider>().As<ILocalizationProvider>().AsSelf();

        }

        public int Order
        {
            get { return -1; }
        }
    }
}
