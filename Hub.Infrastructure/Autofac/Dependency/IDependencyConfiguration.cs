using Autofac;

namespace Hub.Infrastructure.Autofac.Dependency
{
    public interface IDependencyConfiguration
    {
        void Register(ContainerBuilder builder);

        int Order { get; }
    }
}
