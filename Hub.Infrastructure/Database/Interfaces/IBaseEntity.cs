namespace Hub.Infrastructure.Database.Interfaces
{
    public interface IBaseEntity : ICloneable
    {
        long Id { get; set; }
        bool Equals(IBaseEntity other);
        bool Equals(object obj);
        int GetHashCode();
    }
}
