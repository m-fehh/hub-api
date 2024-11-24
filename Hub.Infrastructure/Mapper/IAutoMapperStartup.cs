using AutoMapper;

namespace Hub.Infrastructure.Mapper
{
    public interface IAutoMapperStartup
    {
        void RegisterMaps(IMapperConfigurationExpression cfg);
    }
}
