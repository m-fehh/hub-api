using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(0, DynamicUpdate = true)]
    [Cache(1, Usage = CacheUsage.ReadWrite)]
    public class OrganizationalStructureConfig : BaseEntity, IModificationControl
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "SQ_OrgStructureConfig")]
        public override long Id { get; set; }

        [ManyToOne(Column = "OrganizationalStructureId", NotNull = true)]
        public virtual OrganizationalStructure OrganizationalStructure { get; set; }

        [ManyToOne(Column = "ConfigId", NotNull = true)]
        public virtual OrgStructConfigDefault Config { get; set; }

        [Property(NotNull = true, Length = 300)]
        public virtual string Value { get; set; }

        [Property]
        public virtual DateTime? CreationUTC { get; set; }

        [Property]
        public virtual DateTime? LastUpdateUTC { get; set; }
    }
}
