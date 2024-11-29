using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Database.Runner
{
    [Class(DynamicUpdate = true)]
    public class StartUpDataExecuted : BaseEntity
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "sq_StartUpDataExecuted")]
        public override long Id { get; set; }

        [Property(NotNull = true, Length = 100)]
        public virtual string Name { get; set; }

        [Property(NotNull = true, Column = "CreateDate")]
        public virtual DateTime Date { get; set; }
    }
}
