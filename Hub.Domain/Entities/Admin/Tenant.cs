using System.ComponentModel.DataAnnotations;
using Hub.Shared.Interfaces;
using Hub.Shared.Interfaces.MultiTenant;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entities.Admin
{
    [Class(Table = "tenants")] 
    public class Tenant : BaseEntity, ITenantInfo
    {
        [Id(0, Name = "Id", Column = "id")]
        [Generator(1, Class = "native")]
        public override long Id { get; set; }

        [Property]
        public string Name { get; set; }

        [Property]
        public string Subdomain { get; set; }

        [Property]
        public string Schema { get; set; }

        [Property]
        public bool IsActive { get; set; }

        [Property]
        public string CultureName { get; set; }

        // Mapeamento do EF usando Data Annotations
        [Required]
        [MaxLength(200)]
        [System.ComponentModel.DataAnnotations.Schema.Column("Name", TypeName = "nvarchar(200)")]
        public string NameEF { get; set; }

        [MaxLength(100)]
        public string SubdomainEF { get; set; }

        [MaxLength(50)]
        public string SchemaEF { get; set; }

        public bool IsActiveEF { get; set; }

        [MaxLength(50)]
        public string CultureNameEF { get; set; }
    }
}
