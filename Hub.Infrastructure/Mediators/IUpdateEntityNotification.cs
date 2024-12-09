using Hub.Shared.Enums.Infrastructure;
using Hub.Shared.Interfaces;
using MediatR;

namespace Hub.Infrastructure.Mediators
{
    /// <summary>
    /// Notificação de atualização de uma entidade
    /// </summary>
    /// <typeparam name="T">Entidade atualizada</typeparam>
    public interface IUpdateEntityNotification<T> : INotification
        where T : IBaseEntity
    {
        long Id { get; set; }
        ELogAction Action { get; set; }
    }
}
