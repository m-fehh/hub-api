using Hub.Shared.Enums.Infrastructure;
using Hub.Shared.Interfaces;
using Hub.Shared.Interfaces.Logger;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity.Logger
{
    [Class(DynamicUpdate = true)]
    public class Log : BaseEntity, ILog
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "sq_Log")]
        public override long Id { get; set; }

        [Property(NotNull = true)]
        public virtual DateTime CreateDate { get; set; }

        [ManyToOne(Column = "CreateUserId", ClassType = typeof(PortalUser), NotNull = false)]
        public virtual IUser CreateUser { get; set; }

        [Property(NotNull = true)]
        public virtual long ObjectId { get; set; }

        [Property(NotNull = true)]
        public virtual string ObjectName { get; set; }

        [Property(NotNull = true)]
        public virtual ELogAction Action { get; set; }

        [Property(NotNull = true)]
        public virtual ELogType LogType { get; set; }

        [Property(NotNull = true)]
        public virtual string Message { get; set; }

        [Set(0, Name = "Fields", Cascade = "all-delete-orphan")]
        [Key(1, Column = "LogId")]
        [OneToMany(2, ClassType = typeof(LogField))]
        public virtual ISet<ILogField> Fields { get; set; }

        [ManyToOne(Column = "FatherId", ClassType = typeof(LogField), NotNull = false)]
        public virtual ILogField Father { get; set; }

        [ManyToOne(Column = "OwnerOrgStructId", NotNull = false)]
        public virtual OrganizationalStructure OwnerOrgStruct { get; set; }

        [Property(NotNull = false)]
        public virtual string IpAddress { get; set; }
    }
}
