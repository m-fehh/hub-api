using Hub.Shared.Interfaces;
using Hub.Shared.Interfaces.Logger;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(0, DynamicUpdate = true)]
    [Cache(1, Usage = CacheUsage.ReadWrite)]
    public class AccessRule : BaseEntity, IAccessRule
    {
        public AccessRule()
        {
            ForAdministratorOnly = false;
        }

        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "sq_AccessRule")]
        public override long Id { get; set; }

        [ManyToOne(Column = "ParentId", ClassType = typeof(AccessRule), NotNull = false)]
        public virtual IAccessRule Parent { get; set; }

        [Property(NotNull = true, Length = 150)]
        public virtual string Description { get; set; }

        [Property(NotNull = true, Length = 150)]
        public virtual string KeyName { get; set; }

        [Property(NotNull = true)]
        public virtual bool ForAdministratorOnly { get; set; }

        [Set(0, Name = "Profiles", Table = "ProfileGroup_Rule", Inverse = true)]
        [Key(1, Column = "AccessRuleId")]
        [ManyToMany(2, ClassType = typeof(ProfileGroup), Column = "ProfileGroupId")]
        public virtual ICollection<ProfileGroup> Profiles { get; set; }

        [IgnoreLog]
        [Property]
        public virtual string Tree { get; set; }
    }
}
