using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(0, DynamicUpdate = true)]
    [Cache(1, Usage = CacheUsage.ReadWrite)]
    public class PortalMenuItem : BaseEntity, IPortalMenuItem
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "sq_PortalMenuItem")]
        public override long Id { get; set; }

        [ManyToOne(Column = "MenuId", ClassType = typeof(PortalMenu), NotNull = true)]
        public virtual IPortalMenu Menu { get; set; }

        [ManyToOne(Column = "ParentId", ClassType = typeof(PortalMenuItem), NotNull = false)]
        public virtual IPortalMenuItem Parent { get; set; }

        [ManyToOne(Column = "RuleId", ClassType = typeof(AccessRule), NotNull = true)]
        public virtual IAccessRule Rule { get; set; }

        [Property(Length = 150)]
        public virtual string Url { get; set; }

        [Property(Length = 50)]
        public virtual string IconName { get; set; }

        [Property(Column = "ColOrder")]
        public virtual int Order { get; set; }
    }
}
