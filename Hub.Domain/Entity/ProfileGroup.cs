using Hub.Shared.Interfaces;
using Hub.Shared.Interfaces.Logger;
using Hub.Shared.Log;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(DynamicUpdate = true)]
    [Cache(1, Usage = CacheUsage.ReadWrite)]
    public class ProfileGroup : BaseEntity, IProfileGroup, ILogableEntity
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "sq_ProfileGroup")]
        public override long Id { get; set; }

        [Property(NotNull = true, Length = 150)]
        public virtual string Name { get; set; }

        [DeeperLog]
        [Set(0, Name = "Rules", Table = "ProfileGroup_Rule")]
        [Key(1, Column = "ProfileGroupId")]
        [ManyToMany(2, ClassType = typeof(AccessRule), Column = "AccessRuleId")]
        public virtual ICollection<IAccessRule> Rules { get; set; }

        [Property]
        public virtual bool Administrator { get; set; }
    }
}
