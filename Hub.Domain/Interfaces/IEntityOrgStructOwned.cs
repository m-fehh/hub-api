using Hub.Domain.Entity;
using Hub.Shared.Interfaces;

namespace Hub.Domain.Interfaces
{
    public interface IEntityOrgStructOwned
    {
        public abstract OrganizationalStructure OwnerOrgStruct { get; set; }
    }


    /// <summary>
    /// entidade com visibilidade de domínio virtual no banco
    /// </summary>
    [Serializable]
    public abstract class OrgStructureBaseEntity : BaseEntity, IEntityOrgStructOwned
    {
        public abstract OrganizationalStructure OwnerOrgStruct { get; set; }
    }
}
