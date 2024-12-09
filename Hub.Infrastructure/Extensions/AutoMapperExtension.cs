using AutoMapper;
using Hub.Shared.Interfaces;
using System.Linq.Expressions;

namespace Hub.Infrastructure.Extensions
{
    public static class AutoMapperExtension
    {
        public static void Bidirectional<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression, IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<TDestination, TSource>();
        }

        public static IMappingExpression<TSource, TDestination> Foreign<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> expression,
            Expression<Func<TSource, IBaseEntity>> sourceMember,
            Expression<Func<TDestination, object>> destinationMember)
            where TSource : IBaseEntity
        {
            return expression.ForMember(destinationMember, opt => opt.MapFrom((Func<TSource, TDestination, object>)(

                    (src, dest) =>
                    {
                        var entity = sourceMember.Compile()(src);

                        if (entity == null) return null;

                        return entity.Id;
                    })));
        }

        public static IMappingExpression<TSource, TDestination> Foreign<TSource, TDestination, TResult>(
            this IMappingExpression<TSource, TDestination> expression,
            Expression<Func<TSource, Nullable<long>>> sourceMember,
            Expression<Func<TDestination, TResult>> destinationMember)
            where TDestination : IBaseEntity
        {
            return expression.ForMember(destinationMember,

                (Action<IMemberConfigurationExpression<TSource, TDestination, TResult>>)(opt => opt.MapFrom((Func<TSource, TDestination, TResult>)(

                    (src, dest) =>
                    {
                        var foreignId = sourceMember.Compile()(src);

                        if (foreignId == null) return default(TResult);

                        var destinationType = typeof(TResult);

                        IBaseEntity entity = null;

                        if (destinationType.IsInterface)
                        {
                            entity = (IBaseEntity)Engine.Resolve(destinationType);
                        }
                        else
                        {
                            entity = (IBaseEntity)Activator.CreateInstance(destinationType);
                        }

                        entity.Id = foreignId.Value;

                        return (TResult)entity;
                    }))));
        }
    }
}
