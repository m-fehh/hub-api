using Autofac;
using Hub.Infrastructure.Autofac.Dependency;
using Hub.Infrastructure.Localization;

namespace Hub.API.Configuration
{
    public class DependencyRegistrar : IDependencySetup
    {
        public void Register(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultLocalizationProvider>().As<ILocalizationProvider>().AsSelf();
            builder.RegisterType<HubProvider>().AsSelf().SingleInstance();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
