using AutoMapper;
using Hub.Infrastructure.Mapper;

namespace Hub.Application.ModelMapper.Mapper
{
    /// <summary>
    /// Centraliza todas as configurações de mapemaneto do automapper
    /// </summary>
    public class MapperConfig : IAutoMapperStartup
    {
        /// <summary>
        /// Criação centralizada dos mapeamentos do automapper
        /// </summary>
        public void RegisterMaps(IMapperConfigurationExpression cfg)
        {
        }
    }
}
