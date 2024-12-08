using Hub.Domain.Interfaces;
using Hub.Shared.Enums;
using Hub.Shared.Interfaces;
using Hub.Shared.Interfaces.Logger;
using Hub.Shared.Log;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(0, Table = "PortalUser", DynamicUpdate = true)]
    [Cache(1, Usage = CacheUsage.ReadWrite)]
    public class PortalUser : OrgStructureBaseEntity, IModificationControl, IHubUser, IEntityOrgStructOwned
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "SQ_PortalUser")]
        public override long Id { get; set; }

        [IgnoreLog]
        [ManyToOne(Column = "PersonId")]
        public virtual Person Person { get; set; }

        [Property(NotNull = true, Length = 150)]
        public virtual string Name { get; set; }

        [Property(NotNull = true, Length = 150)]
        public virtual string Email { get; set; }

        [Property(NotNull = true, Length = 50)]
        public virtual string Login { get; set; }

        [IgnoreLog]
        [Property(NotNull = true, Length = 50)]
        public virtual string Password { get; set; }

        [IgnoreLog]
        [Property(NotNull = false, Length = 50)]
        public virtual string TempPassword { get; set; }

        [Property(Length = 100)]
        public virtual string CpfCnpj { get; set; }

        [Property(NotNull = false)]
        public virtual EGender? Gender { get; set; }

        [Property(NotNull = false)]
        public virtual DateTime? BirthDate { get; set; }

        [Property(NotNull = false)]
        public virtual string AreaCode { get; set; }

        [Property(NotNull = false)]
        public virtual string PhoneNumber { get; set; }

        [IgnoreLog]
        public virtual bool ChangingPass { get; set; }

        [Property(NotNull = false, Length = 100)]
        public virtual string Keyword { get; set; }

        [Property]
        public virtual bool Inactive { get; set; }

        [ManyToOne(Column = "ProfileId", ClassType = typeof(ProfileGroup), NotNull = true)]
        public virtual IProfileGroup Profile { get; set; }

        [ManyToOne(Column = "OwnerOrgStructId", NotNull = true)]
        public override OrganizationalStructure OwnerOrgStruct { get; set; }

        [ManyToOne(Column = "DefaultOrgStructureId", NotNull = true)]
        public virtual OrganizationalStructure DefaultOrgStructure { get; set; }

        [DeeperLog]
        [Set(0, Name = "OrganizationalStructures", Table = "PortalUser_OrgStructure")]
        [Key(1, Column = "PortalUserId")]
        [ManyToMany(2, ClassType = typeof(OrganizationalStructure), Column = "StructureId")]
        public virtual ICollection<OrganizationalStructure> OrganizationalStructures { get; set; }


        [Property(NotNull = false, Length = 100)]
        public virtual string ExternalCode { get; set; }

        [Property(NotNull = false)]
        public virtual DateTime? LastAccessDate { get; set; }

        [IgnoreLog]
        [Property(NotNull = false)]
        public virtual DateTime? CreationUTC { get; set; }

        [IgnoreLog]
        [Property(NotNull = false)]
        public virtual DateTime? LastUpdateUTC { get; set; }

        public virtual ICollection<IAccessRule> AllRules { get { return Profile?.Rules; } }
    }
}
