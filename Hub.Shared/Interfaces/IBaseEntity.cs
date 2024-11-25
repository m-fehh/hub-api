namespace Hub.Shared.Interfaces
{
    public interface IBaseEntity : ICloneable
    {
        long Id { get; set; }
        bool Equals(IBaseEntity other);
        bool Equals(object obj);
        int GetHashCode();
    }


    public interface IModificationControl : IBaseEntity
    {
        DateTime? CreationUTC { get; set; }
        DateTime? LastUpdateUTC { get; set; }
    }

    public interface IListItemEntity
    {
        bool DeleteFromList { get; set; }
    }

    /// <summary>
    /// Implementação padrão da interface que representa uma entidade no banco
    /// </summary>
    [Serializable]
    public abstract class BaseEntity : IBaseEntity
    {
        public abstract Int64 Id { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as BaseEntity);
        }

        private static bool IsTransient(IBaseEntity obj)
        {
            return obj != null && Equals(obj.Id, default(Int64));
        }

        private Type GetUnproxiedType()
        {
            return GetType();
        }

        public virtual bool Equals(IBaseEntity other)
        {
            if (other == null)
                return false;

            if (!(other is BaseEntity))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (!IsTransient(this) &&
                !IsTransient(other) &&
                Equals(Id, other.Id))
            {
                var otherType = ((BaseEntity)other).GetUnproxiedType();
                var thisType = GetUnproxiedType();
                return thisType.IsAssignableFrom(otherType) ||
                        otherType.IsAssignableFrom(thisType);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (Equals(Id, default(Int64)))
                return base.GetHashCode();
            return Id.GetHashCode();
        }

        public static bool operator ==(BaseEntity x, BaseEntity y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(BaseEntity x, BaseEntity y)
        {
            return !(x == y);
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
