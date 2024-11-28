using NHibernate.Mapping.Attributes;
using Hub.Shared.Interfaces;
using Hub.Shared.Interfaces.MultiTenant;

namespace Hub.Domain.Entities.Admin
{
    [Class(Table = "tenants")]
    public class Tenants : BaseEntity, ITenantInfo
    {
        [Id(0, Name = "Id", Column = "id")]
        [Generator(1, Class = "native")]
        public override long Id { get; set; }

        [Property(NotNull = true, Length = 200)]
        public virtual string Name { get; set; }

        [Property]
        public virtual string Subdomain { get; set; }

        [Property(NotNull = true)]
        public virtual bool IsActive { get; set; }

        [Property]
        public virtual string CultureName { get; set; }
    }
}

