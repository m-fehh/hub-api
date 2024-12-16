using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(0, DynamicUpdate = true)]
    [Cache(1, Usage = CacheUsage.ReadWrite)]
    public class PortalMenu : BaseEntity, IPortalMenu
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "sq_PortalMenu")]
        public override long Id { get; set; }

        [Property(NotNull = true, Length = 150)]
        public virtual string Name { get; set; }

        [Bag(0, Name = "Itens", Inverse = true)]
        [Key(1, Column = "MenuId")]
        [OneToMany(2, ClassType = typeof(PortalMenuItem))]
        public virtual IList<IPortalMenuItem> Itens { get; set; }
    }
}
