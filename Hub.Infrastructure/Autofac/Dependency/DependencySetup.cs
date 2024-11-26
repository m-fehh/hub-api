using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Hub.Domain;
using Hub.Infrastructure.Database;
using Hub.Infrastructure.Localization;
using Hub.Infrastructure.MultiTenant;
using Hub.Infrastructure.Nominator;
using Hub.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Hub.Infrastructure.Autofac.Dependency
{
    public class DependencySetup : IDependencySetup
    {
        public void Register(ContainerBuilder builder)
        {
            // Registro do DbContext (DatabaseContext)
            builder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
                // Aqui você pode configurar a string de conexão e outras opções
                optionsBuilder.UseSqlServer(Engine.ConnectionString("default"));

                return new DatabaseContext(optionsBuilder.Options);
            })
            .As<DatabaseContext>()
            .InstancePerLifetimeScope();

            // Registra os serviços do contêiner
            builder.RegisterType<TenantLifeTimeScope>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<DefaultTenantManager>().As<ITenantManager>().SingleInstance();
            builder.RegisterType<ConnectionStringBaseConfigurator>().AsSelf().SingleInstance();
            builder.RegisterType<DefaultLocalizationProvider>().As<ILocalizationProvider>().AsSelf();
            builder.RegisterType<NominatorManager>().As<INominatorManager>().SingleInstance();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).OnActivating(ActivingRepository);
            builder.RegisterType<DefaultOrmConfiguration>().As<IOrmConfiguration>().SingleInstance();
            builder.RegisterType<DefaultOrmConfiguration>().AsSelf().SingleInstance();

            // Registrar todos os seeders
            RegisterSeeders(builder);

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

        // Método para registrar todos os seeders automaticamente
        private void RegisterSeeders(ContainerBuilder builder)
        {
            builder.RegisterType<SeederManager>().AsSelf().SingleInstance();

            // Encontra todas as classes que implementam ISeeder
            var seederTypes = Assembly.GetExecutingAssembly()
                                      .GetTypes()
                                      .Where(t => typeof(ISeeder).IsAssignableFrom(t) && !t.IsAbstract);

            // Registra cada seeder no container
            foreach (var seederType in seederTypes)
            {
                builder.RegisterType(seederType).As<ISeeder>().InstancePerLifetimeScope();
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
