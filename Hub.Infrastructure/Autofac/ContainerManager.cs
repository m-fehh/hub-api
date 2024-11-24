using Autofac;
using Autofac.Builder;
using Hub.Infrastructure.Autofac.Dependency;
using Hub.Shared.Enums.Infra;

namespace Hub.Infrastructure.Autofac
{
    public class ContainerManager
    {
        private IContainer _container;
        private readonly ContainerBuilder _builder;

        public ContainerManager(IList<IDependencySetup> dependencyRegistrars, ContainerBuilder containerBuilder = null)
        {
            _builder = containerBuilder ?? new ContainerBuilder();

            var drInstances = dependencyRegistrars.OrderBy(d => d.Order).ToList();

            foreach (var dependencyRegistrar in drInstances)
            {
                dependencyRegistrar.Register(_builder);
            }

            if (containerBuilder == null)
            {
                _container = _builder.Build();
            }
        }

        public IContainer Container
        {
            get { return _container; }
            set { _container = value; }
        }

        public T Resolve<T>(string key = "", ILifetimeScope scope = null)
        {
            if (scope == null)
            {
                //no scope specified
                scope = Scope();
            }
            if (string.IsNullOrEmpty(key))
            {
                return scope.Resolve<T>();
            }
            return scope.ResolveKeyed<T>(key);
        }

        public object Resolve(Type type, ILifetimeScope scope = null)
        {
            if (scope == null)
            {
                //no scope specified
                scope = Scope();
            }
            return scope.Resolve(type);
        }

        public T[] ResolveAll<T>(string key = "", ILifetimeScope scope = null)
        {
            if (scope == null)
            {
                //no scope specified
                scope = Scope();
            }
            if (string.IsNullOrEmpty(key))
            {
                return scope.Resolve<IEnumerable<T>>().ToArray();
            }
            return scope.ResolveKeyed<IEnumerable<T>>(key).ToArray();
        }

        public object ResolveOptional(Type serviceType, ILifetimeScope scope = null)
        {
            if (scope == null)
            {
                //no scope specified
                scope = Scope();
            }
            return scope.ResolveOptional(serviceType);
        }

        public ILifetimeScope Scope()
        {
            try
            {
                return Container.BeginLifetimeScope("AutofacWebRequest");
            }
            catch
            {
                return Container;
            }
        }
    }

    public static class ContainerManagerExtensions
    {
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PerLifeStyle<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, EComponentLifeStyle lifeStyle)
        {
            switch (lifeStyle)
            {
                case EComponentLifeStyle.LifetimeScope:
                    return builder.InstancePerLifetimeScope();
                case EComponentLifeStyle.Transient:
                    return builder.InstancePerDependency();
                case EComponentLifeStyle.Singleton:
                    return builder.SingleInstance();
                default:
                    return builder.SingleInstance();
            }
        }
    }
}
