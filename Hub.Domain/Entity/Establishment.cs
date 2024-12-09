using Hub.Shared.Interfaces.Logger;
using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(0, DynamicUpdate = true)]
    [Cache(1, Usage = CacheUsage.ReadWrite)]
    public class Establishment : BaseEntity, ILogableEntity, IModificationControl
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "SQ_Establishment")]
        public override long Id { get; set; }

        [ManyToOne(Column = "OrganizationalStructureId", NotNull = true)]
        public virtual OrganizationalStructure OrganizationalStructure { get; set; }

        [Property(NotNull = true, Length = 20)]
        public virtual string CNPJ { get; set; }

        [Property(NotNull = true, Length = 500)]
        public virtual string SocialName { get; set; }

        [Property(NotNull = true, Length = 15)]
        public virtual string PostalCode { get; set; }

        [Property(NotNull = false, Length = 15)]
        public virtual string OpeningTime { get; set; }

        [Property(NotNull = false, Length = 15)]
        public virtual string ClosingTime { get; set; }

        [Property(NotNull = false, Length = 50)]
        public virtual string PhoneNumber { get; set; }

        [Property]
        public virtual DateTime? SystemStartDate { get; set; }

        [IgnoreLog]
        [Property(NotNull = false)]
        public virtual DateTime? CreationUTC { get; set; }

        [IgnoreLog]
        [Property(NotNull = false)]
        public virtual DateTime? LastUpdateUTC { get; set; }
    }
}
