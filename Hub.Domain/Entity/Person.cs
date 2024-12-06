using Hub.Shared.Enums;
using Hub.Shared.Interfaces;
using Hub.Shared.Interfaces.Logger;
using NHibernate.Mapping.Attributes;

namespace Hub.Domain.Entity
{
    public class Person : BaseEntity, IModificationControl
    {
        [Id(0, Name = "Id", Type = "Int64")]
        [Generator(1, Class = "native")]
        [Param(2, Name = "sequence", Content = "sq_Person")]
        public override long Id { get; set; }

        [Property(NotNull = true, Length = 20)]
        public virtual string Document { get; set; }

        [Property(NotNull = true, Length = 500)]
        public virtual string Name { get; set; }

        [Property(NotNull = false)]
        public virtual EGender? Gender { get; set; }

        [Property(NotNull = false)]
        public virtual DateTime? BirthDate { get; set; }

        [Property(NotNull = false)]
        public virtual string AreaCode { get; set; }

        [Property(NotNull = false)]
        public virtual string PhoneNumber { get; set; }

        [IgnoreLog]
        [Property(NotNull = false)]
        public virtual DateTime? CreationUTC { get; set; }

        [IgnoreLog]
        [Property(NotNull = false)]
        public virtual DateTime? LastUpdateUTC { get; set; }
    }
}
