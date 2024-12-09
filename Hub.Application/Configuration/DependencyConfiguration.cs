using Autofac;
using Hub.Application.ModelMapper;
using Hub.Application.Services;
using Hub.Application.Services.Admin;
using Hub.Domain.Database.Runner;
using Hub.Domain.Entity;
using Hub.Domain.Interfaces;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Autofac.Dependency;
using Hub.Infrastructure.Database.NhManagement.Migrations;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Hangfire;
using Hub.Infrastructure.Localization;
using Hub.Infrastructure.Mapper;
using Hub.Infrastructure.Security;
using Hub.Infrastructure.Web;
using Hub.Shared.DataConfiguration.Admin;
using Hub.Shared.Interfaces;
using MediatR;

namespace Hub.Application.Configuration
{
    public class DependencyConfiguration : IDependencyConfiguration
    {
        public void Register(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultLocalizationProvider>().As<ILocalizationProvider>().AsSelf();

            builder.RegisterType<DbMigrator>().AsSelf();
            builder.RegisterType<MigrationRunner>().As<IMigrationRunner>();
            builder.RegisterType<VersionManager>().As<IVersionManager>();

            builder.RegisterGeneric(typeof(AutoModelEntityMapper<,>)).As(typeof(IModelEntityMapper<,>)).AsSelf().InstancePerLifetimeScope();

            #region Mediators

            builder.RegisterType<Mediator>().As<IMediator>().SingleInstance();

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });

            #endregion

            #region Services 
            builder.RegisterType<PortalAccessTokenProvider>().As<IAccessTokenProvider>().InstancePerLifetimeScope();
            builder.RegisterType<HangfireHelper>().As<IHangfireHelper>().SingleInstance();

            builder.RegisterType<UserService>().As<ICrudService<PortalUser>>().As<ISecurityProvider>().As<ISecurityProviderTemp>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<PortalUserPassHistoryService>().As<ICrudService<PortalUserPassHistory>>().AsSelf();

            builder.RegisterType<ProfileGroupService>().As<ICrudService<ProfileGroup>>();
            builder.RegisterType<ProfileGroupService>().AsSelf();

            builder.RegisterType<AccessRuleService>().As<ICrudService<AccessRule>>();
            builder.RegisterType<AccessRuleService>().AsSelf();

            builder.RegisterType<OrganizationalStructureService>().As<ICrudService<OrganizationalStructure>>();
            builder.RegisterType<OrganizationalStructureService>().AsSelf();
            builder.RegisterType<OrganizationalStructureService>().As<IOrgStructBasedService>();
            builder.RegisterType<OrganizationalStructureConfigService>().As<ICrudService<OrganizationalStructureConfig>>();
            builder.RegisterType<OrganizationalStructureConfigService>().AsSelf();

            builder.RegisterType<PortalUserFingerprintService>().As<ICrudService<PortalUserFingerprint>>();
            builder.RegisterType<PortalUserFingerprintService>().AsSelf();

            builder.RegisterType<EstablishmentService>().As<ICrudService<Establishment>>();
            builder.RegisterType<EstablishmentService>().AsSelf();

            builder.RegisterType<PersonService>().As<ICrudService<Person>>();
            builder.RegisterType<PersonService>().AsSelf();

            builder.RegisterType<UserKeywordService>().AsSelf();

            builder.RegisterType<UserProfileControlAccessService>().As<IUserProfileControlAccessService>();
            builder.RegisterType<UserProfileControlAccessService>().AsSelf();

            builder.RegisterType<LoginService>().AsSelf();


            builder.RegisterType<ProfileGroup>().As<IProfileGroup>();



            builder.RegisterType<TenantService>().As<ICrudService<Tenants>>().AsSelf().InstancePerLifetimeScope();

            #endregion
        }

        public int Order
        {
            get { return 5; }
        }
    }
}
