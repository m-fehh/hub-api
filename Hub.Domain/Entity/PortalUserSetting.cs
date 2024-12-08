using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(DynamicUpdate = true)]
    [Cache(1, Usage = CacheUsage.ReadWrite)]
    public class PortalUserSetting : BaseEntity
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "SQ_PortalUserSetting")]
        public override long Id { get; set; }

        [ManyToOne(Column = "PortalUserId", NotNull = true)]
        public virtual PortalUser PortalUser { get; set; }

        [Property(NotNull = true, Length = 150)]
        public virtual string Name { get; set; }

        [Property(NotNull = true, Length = 4000)]
        public virtual string Value { get; set; }
    }
}