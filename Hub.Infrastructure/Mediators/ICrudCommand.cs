using Hub.Infrastructure.Database.Services;
using Hub.Shared.Interfaces;
using MediatR;

namespace Hub.Infrastructure.Mediators
{
    /// <summary>
    /// representa um comando de uma operação create de um CRUD
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    public interface ICreateCommand<T, E> : IRequest<E>
        where T : ICrudModel
        where E : IBaseEntity

    {
        T Model { get; set; }
    }

    /// <summary>
    /// representa um comando de uma operação update de um CRUD
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    public interface IUpdateCommand<T, E> : IRequest<E>
        where T : ICrudModel
        where E : IBaseEntity

    {
        T Model { get; set; }
    }

    /// <summary>
    /// representa um comando de uma operação update de um CRUD
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    public interface IDeleteCommand<E> : IRequest<bool>
        where E : IBaseEntity
    {
        long Id { get; set; }
    }
}
