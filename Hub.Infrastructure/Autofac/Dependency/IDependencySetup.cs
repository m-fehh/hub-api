using Autofac;

namespace Hub.Infrastructure.Autofac.Dependency
{
    public interface IDependencySetup
    {
        void Register(ContainerBuilder builder);

        int Order { get; }
    }
}
