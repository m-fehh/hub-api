using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(0, DynamicUpdate = true)]
    [Cache(1, Usage = CacheUsage.ReadWrite)]
    public class OrgStructConfigDefault : BaseEntity
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "SQ_OrgStructConfigDefault")]
        public override long Id { get; set; }

        [Property(NotNull = true, Length = 150)]
        public virtual string Name { get; set; }

        [Property(NotNull = true, Length = 300)]
        public virtual string DefaultValue { get; set; }

        [Property(NotNull = true, Length = 150)]
        public virtual string ConfigType { get; set; }

        [Property(NotNull = true)]
        public virtual bool ApplyToRoot { get; set; }

        [Property(NotNull = true)]
        public virtual bool ApplyToDomain { get; set; }

        [Property(NotNull = true)]
        public virtual bool ApplyToLeaf { get; set; }

        [Property(NotNull = true, Length = 150)]
        public virtual string GroupName { get; set; }

        [Property(NotNull = false, Length = 150)]
        public virtual string SearchName { get; set; }

        [Property(NotNull = false, Length = 150)]
        public virtual string SearchExtraCondition { get; set; }

        [Property(NotNull = false, Length = int.MaxValue)]
        public virtual string Options { get; set; }

        [Property(NotNull = false, Length = 150)]
        public virtual string Legend { get; set; }

        [ManyToOne(Column = "OrgStructConfigDefaultDependencyId")]
        public virtual OrgStructConfigDefault OrgStructConfigDefaultDependency { get; set; }

        [Property(NotNull = false)]
        public virtual int? MaxLength { get; set; }
    }
}
