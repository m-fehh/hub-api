using Hub.Shared.Interfaces;

namespace Hub.Domain.Interfaces
{
    public interface IOrgStructBasedService
    {
        void LinkOwnerOrgStruct(IEntityOrgStructOwned entity);

        bool AllowChanges<TEntity>(TEntity entity, bool thowsException = true) where TEntity : IBaseEntity, IEntityOrgStructOwned;
    }
}
