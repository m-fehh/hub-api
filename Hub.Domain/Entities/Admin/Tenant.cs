//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using Hub.Shared.Interfaces;
//using Hub.Shared.Interfaces.MultiTenant;
//using NHibernate.Mapping.Attributes;

//namespace Hub.Domain.Entities.Admin
//{
//    [Class(Table = "tenants")]
//    public class Tenants : BaseEntity, ITenantInfo
//    {
//        [System.ComponentModel.DataAnnotations.Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        [Id(0, Name = "Id", Column = "id")]
//        [Generator(1, Class = "native")]
//        public override long Id { get; set; }

//        [Required]
//        [MaxLength(200)]
//        [Property(Column = "Name", NotNull = true, Length = 200)]
//        public virtual string Name { get; set; }

//        [MaxLength(100)]
//        [Property(Column = "Subdomain", Length = 50)]
//        public virtual string Subdomain { get; set; }

//        [MaxLength(50)]
//        [Property(Column = "SchemaName", Length = 30)]
//        public virtual string SchemaName { get; set; }

//        [Property(Column = "IsActive", NotNull = true)]
//        public virtual bool IsActive { get; set; }

//        [MaxLength(50)]
//        [Property(Column = "CultureName", Length = 50)]
//        public virtual string CultureName { get; set; }
//    }
//}


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

        [Property]
        public virtual string SchemaName { get; set; }

        [Property(NotNull = true)]
        public virtual bool IsActive { get; set; }

        [Property]
        public virtual string CultureName { get; set; }
    }
}

