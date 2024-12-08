using Hub.Shared.Interfaces.Logger;
using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity.Logger
{
    [Class(DynamicUpdate = true)]
    public class LogField : BaseEntity, ILogField
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "sq_LogField")]
        public override long Id { get; set; }

        [ManyToOne(Column = "LogId", ClassType = typeof(Log), NotNull = true)]
        public virtual ILog Log { get; set; }

        [Property(NotNull = true)]
        public virtual string PropertyName { get; set; }

        [Property(NotNull = false)]
        public virtual string OldValue { get; set; }

        [Property(NotNull = false)]
        public virtual string NewValue { get; set; }

        [Set(0, Name = "Childs", Cascade = "all-delete-orphan")]
        [Key(1, Column = "FatherId")]
        [OneToMany(2, ClassType = typeof(Log))]
        public virtual ISet<ILog> Childs { get; set; }
    }
}
