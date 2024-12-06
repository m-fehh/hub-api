using Hub.Shared.Interfaces.Logger;
using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(0, DynamicUpdate = true)]
    [Cache(1, Usage = CacheUsage.ReadWrite)]
    public class OrganizationalStructure : BaseEntity, ILogableEntity, IModificationControl
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "sq_OrganizationalStructure")]
        public override long Id { get; set; }

        [Property(NotNull = true, Length = 10)]
        public virtual string Abbrev { get; set; }

        [Property(NotNull = true, Length = 150)]
        public virtual string Description { get; set; }

        [Property]
        public virtual bool Inactive { get; set; }

        [Property]
        public virtual bool IsRoot { get; set; }

        [Property]
        public virtual bool IsLeaf { get; set; }

        [Property]
        public virtual bool IsDomain { get; set; }

        [ManyToOne(Column = "FatherId", NotNull = false)]
        public virtual OrganizationalStructure Father { get; set; }

        [Set(0, Inverse = true)]
        [Key(1, Column = "FatherId")]
        [OneToMany(2, ClassType = typeof(OrganizationalStructure))]
        public virtual ICollection<OrganizationalStructure> Childrens { get; set; }

        [Set(0, Name = "Users", Table = "PortalUser_OrgStructure", Inverse = true)]
        [Key(1, Column = "StructureId")]
        [ManyToMany(2, ClassType = typeof(PortalUser), Column = "UserId")]
        public virtual ICollection<PortalUser> Users { get; set; }

        [IgnoreLog]
        [Property]
        public virtual string Tree { get; set; }

        [Property(NotNull = false, Length = 100)]
        public virtual string ExternalCode { get; set; }

        [IgnoreLog]
        [Property(NotNull = false)]
        public virtual DateTime? CreationUTC { get; set; }

        [IgnoreLog]
        [Property(NotNull = false)]
        public virtual DateTime? LastUpdateUTC { get; set; }
    }
}
