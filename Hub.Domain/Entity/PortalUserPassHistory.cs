using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(0, Table = "PortalUserPassHistory", DynamicUpdate = true)]
    [Cache(1, Usage = CacheUsage.ReadWrite)]
    public class PortalUserPassHistory : BaseEntity
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "SQ_PortalUserPassHistory")]
        public override Int64 Id { get; set; }

        [ManyToOne(Column = "UserId")]
        public virtual PortalUser User { get; set; }

        [Property(Length = 100)]
        public virtual string Password { get; set; }

        [Property(NotNull = true)]
        public virtual DateTime CreationUTC { get; set; }

        [Property(NotNull = false)]
        public virtual DateTime? ExpirationUTC { get; set; }

    }
}
