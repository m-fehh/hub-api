using Autofac;
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

        }

        public int Order
        {
            get { return -1; }
        }
    }
}
