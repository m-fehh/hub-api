using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Hub.Infrastructure.Database;
using Hub.Infrastructure.Localization;
using Hub.Infrastructure.MultiTenant;
using Hub.Infrastructure.Nominator;

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
            builder.RegisterType<NominatorManager>().As<INominatorManager>().SingleInstance();

            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).OnActivating(ActivingRepository);

            builder.RegisterType<DefaultOrmConfiguration>().As<IOrmConfiguration>().SingleInstance();
            builder.RegisterType<DefaultOrmConfiguration>().AsSelf().SingleInstance();

            void ActivingRepository(IActivatingEventArgs<object> e)
            {
                var typeToLookup = e.Instance.GetType().GetGenericArguments()[0];

                if (typeToLookup.IsInterface)
                {
                    try
                    {
                        var foundEntry = e.Context.Resolve(typeToLookup);

                        if (foundEntry != null)
                        {
                            ((ISetType)e.Instance).SetType(foundEntry.GetType());
                        }
                    }
                    catch (ComponentNotRegisteredException)
                    {
                    }

                }
            }
        }

        public int Order
        {
            get { return -1; }
        }
    }

    public interface ISetType
    {
        void SetType(Type resolvedType);
    }
}
