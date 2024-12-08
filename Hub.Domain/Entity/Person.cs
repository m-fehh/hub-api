using Hub.Domain.Interfaces;
using Hub.Shared.Enums;
using Hub.Shared.Interfaces;
using Hub.Shared.Interfaces.Logger;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(DynamicUpdate = true)]
    public class Person : BaseEntity, IEntityOrgStructOwned, IModificationControl
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "SQ_Person")]
        public override long Id { get; set; }

        [Property(NotNull = true, Length = 20)]
        public virtual string Document { get; set; }

        [Property(NotNull = true, Length = 500)]
        public virtual string Name { get; set; }

        [Property(NotNull = false, Length = 100)]
        public virtual string ExternalCode { get; set; }

        [ManyToOne(Column = "OwnerOrgStructId", NotNull = true)]
        public virtual OrganizationalStructure OwnerOrgStruct { get; set; }

        [Set(0, Name = "OrganizationalStructures", Table = "Person_OrgStructure")]
        [Key(1, Column = "PersonId")]
        [ManyToMany(2, ClassType = typeof(OrganizationalStructure), Column = "StructureId")]
        public virtual ICollection<OrganizationalStructure> OrganizationalStructures { get; set; }

        [IgnoreLog]
        [Property(NotNull = false)]
        public virtual DateTime? CreationUTC { get; set; }

        [IgnoreLog]
        [Property(NotNull = false)]
        public virtual DateTime? LastUpdateUTC { get; set; }
    }
}
