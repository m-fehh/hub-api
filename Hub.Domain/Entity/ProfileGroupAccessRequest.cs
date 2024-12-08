using Hub.Shared.Enums;
using Hub.Shared.Interfaces.Logger;
using Hub.Shared.Interfaces;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    [Class(DynamicUpdate = true)]
    public class ProfileGroupAccessRequest : BaseEntity, IModificationControl, ILogableEntity
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "SQ_ProfileGroupAccessRequest")]
        public override long Id { get; set; }

        [ManyToOne(Column = "TemporaryProfileId", ClassType = typeof(ProfileGroup), NotNull = false)]
        public virtual ProfileGroup TemporaryProfile { get; set; }

        [ManyToOne(Column = "PortalUserRequestId", ClassType = typeof(PortalUser), NotNull = true)]
        public virtual PortalUser PortalUserRequest { get; set; }

        [ManyToOne(Column = "PortalUserReceivedId", ClassType = typeof(PortalUser), NotNull = true)]
        public virtual PortalUser PortalUserReceived { get; set; }

        [ManyToOne(Column = "ProfileGroupRequestId", ClassType = typeof(ProfileGroup), NotNull = true)]
        public virtual ProfileGroup ProfileGroupRequest { get; set; }

        [ManyToOne(Column = "ProfileGroupReceivedId", ClassType = typeof(ProfileGroup), NotNull = true)]
        public virtual ProfileGroup ProfileGroupReceived { get; set; }

        [Property(NotNull = true)]
        public virtual DateTime InitValidity { get; set; }

        [Property(NotNull = true)]
        public virtual DateTime EndValidity { get; set; }

        [Property(NotNull = false)]
        public virtual EProfileGroupAccessRequestStatus Status { get; set; }

        [Property(NotNull = true)]
        public virtual bool Inactive { get; set; }

        [Property(NotNull = false, Length = 100)]
        public virtual string ProfileCodeTemporary { get; set; }

        [Property(NotNull = false)]
        public virtual DateTime? CreationUTC { get; set; }

        [Property(NotNull = false)]
        public virtual DateTime? LastUpdateUTC { get; set; }
    }
}
