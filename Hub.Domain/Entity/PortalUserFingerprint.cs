using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    /// <summary>
    /// Tabela para armazenar informações diversas no momento do login
    /// <see href="https://dev.azure.com/evuptec/EVUP/_workitems/edit/17365/">Link do PBI</see>
    /// </summary>
    [Class(DynamicUpdate = true)]
    public class PortalUserFingerprint : BaseEntity, IModificationControl
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "SQ_PortalUserFingerprint")]
        public override long Id { get; set; }

        [ManyToOne(Column = "UserId", NotNull = true)]
        public virtual PortalUser PortalUser { get; set; }

        [Property(NotNull = false)]
        public virtual string OS { get; set; }

        [Property(NotNull = false)]
        public virtual string BrowserName { get; set; }

        [Property(NotNull = false)]
        public virtual string BrowserInfo { get; set; }

        [Property(NotNull = false)]
        public virtual double? Lat { get; set; }

        [Property(NotNull = false)]
        public virtual double? Lng { get; set; }

        [Property(NotNull = false)]
        public virtual string IpAddress { get; set; }

        [Property(NotNull = false)]
        public virtual DateTime? CreationUTC { get; set; }

        [Property(NotNull = false)]
        public virtual DateTime? LastUpdateUTC { get; set; }

        [Property(NotNull = false)]
        public virtual bool? CookieEnabled { get; set; }

    }
}
