using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Hub.Domain.SQLManagement;
using Hub.Infrastructure.Database;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Logger.Interfaces;
using Hub.Infrastructure.MultiTenant;
using Hub.Infrastructure.Nominator;
using Hub.Shared.Interfaces.MultiTenant;
using Hub.Shared.Log;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hub.Infrastructure.Autofac.Dependency
{
    public class DependencySetup : IDependencySetup
    {
        public void Register(ContainerBuilder builder)
        {
            builder.RegisterType<LogManager>().As<ILogManager>().SingleInstance();

            builder.RegisterType<NominatorManager>().As<INominatorManager>().SingleInstance();

            //builder.RegisterType<SendMail>().As<ISendMail>().SingleInstance();

            builder.RegisterType<IgnoreLogScope>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<IgnoreModificationControl>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<TenantLifeTimeScope>().AsSelf().InstancePerLifetimeScope();

            //builder.RegisterType<AzureCloudStorageManager>().As<IAzureCloudStorageManager>().SingleInstance();
            //builder.RegisterType<AzureCloudStorageManager>().AsSelf().SingleInstance();

            //builder.RegisterType<RedisService>().AsSelf().SingleInstance();
            //builder.RegisterType<RedisService>().As<IRedisService>().SingleInstance();

            //builder.RegisterType<RedLockManager>().AsSelf().SingleInstance();
            //builder.RegisterType<RedLockManager>().As<ILockManager>().SingleInstance();

            //builder.RegisterType<ServiceBusManager>().AsSelf().SingleInstance();

            //builder.RegisterGeneric(typeof(AzureTableStorageManager<>)).AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<NhRepository>().AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(NhRepository<>)).As(typeof(IRepository<>)).OnActivating(ActivingRepository);
            //builder.RegisterGeneric(typeof(SchNoSqlRepository<>)).AsSelf();

            builder.RegisterType<NhDatabaseInformation>().As<IDatabaseInformation>();

            builder.RegisterType<NhLifetimeScopeSession>().As<ILifetimeScopeSession>().InstancePerLifetimeScope();

            builder.RegisterType<NhStartSessionFactory>().As<INhStartSessionFactory>().SingleInstance();

            builder.RegisterType<NhStatelessSessionScope>().As<INhStatelessSessionScope>();

            //builder.RegisterType<CosmosDbConnectionProvider>().As<INoSqlConnectionProvider>().SingleInstance();
            //builder.RegisterType<CosmosDbConnectionProvider>().AsSelf().SingleInstance();

            builder.RegisterType<DefaultOrmConfiguration>().As<IOrmConfiguration>().SingleInstance();
            builder.RegisterType<DefaultOrmConfiguration>().AsSelf().SingleInstance();

            //builder.RegisterType<ApiRequestService>().AsImplementedInterfaces().AsSelf().SingleInstance();

            //builder.RegisterType<RandomGeneration>().As<IRandomGeneration>().SingleInstance();

            //builder.RegisterGeneric(typeof(AzureSearchManager<>)).AsSelf().InstancePerLifetimeScope();
            //builder.RegisterType<RequestParametersBuilder>().AsImplementedInterfaces();
            //builder.RegisterType<RequestParametersBuilder>().AsSelf();

            //builder.RegisterType<EngineInitializationParametersBuilder>().AsImplementedInterfaces();
            //builder.RegisterType<EngineInitializationParametersBuilder>().AsSelf();

            //builder.RegisterType<CacheManager>().AsSelf().SingleInstance();
            builder.RegisterType<DefaultTenantManager>().As<ITenantManager>().SingleInstance();

            //builder.RegisterType<CurrentTimezone>().As<ICurrentTimezone>().SingleInstance();

            //builder.RegisterType<UserVM>().As<IUser>();

            //builder.RegisterType<MongoManager>().AsSelf().SingleInstance();

            //builder.RegisterGeneric(typeof(MongoRepository<>)).AsSelf().InstancePerLifetimeScope();

            //builder.RegisterType<DatabaseConnectionProvider>().AsSelf().InstancePerLifetimeScope();

            //builder.RegisterType<ConfigurationService>().AsSelf();
            //builder.RegisterType<ConfigurationService>().AsImplementedInterfaces();

            //builder.RegisterType<ConfigurationManagerConfigProvider>().AsSelf();
            //builder.RegisterType<EnvironmentConfigProvider>().AsSelf();

            //builder.RegisterGeneric(typeof(ModelEntityMapper<,>)).AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<NhReadOnlySessionScope>().As<INhReadOnlySessionScope>().InstancePerLifetimeScope();

            builder.RegisterType<ConnectionStringBaseConfigurator>().AsSelf().SingleInstance();

            //builder.RegisterType<StringEncrypter>().As<IStringEncrypter>().SingleInstance();

            //builder.RegisterType<AccessTokenProvider>().As<IAccessTokenProvider>().SingleInstance();

            //builder.RegisterType<Mediator>().As<IMediator>().SingleInstance();

            //builder.RegisterType<BackgroundJobManager>().AsSelf().InstancePerLifetimeScope();

            // Registro do DbContext (AdminContext) com ConnectionString


            builder.Register(c =>
            {
                var csb = Engine.ConnectionString("adm");
                var optionsBuilder = new DbContextOptionsBuilder<AdminContext>().UseSqlServer(csb);
                return new AdminContext(optionsBuilder.Options);
            }).AsSelf().InstancePerLifetimeScope();


            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
        }
        void ActivingRepository(IActivatingEventArgs<object> e)
        {
            var typeToLookup = e.Instance.GetType().GetGenericArguments()[0];

            if (typeToLookup.IsInterface)
            {
                //var foundEntry = e.Context.ComponentRegistry.RegistrationsFor(new TypedService(typeToLookup)).SingleOrDefault();

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
