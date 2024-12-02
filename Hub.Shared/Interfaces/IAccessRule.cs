namespace Hub.Shared.Interfaces
{
    public interface IAccessRule : IBaseEntity, ICloneable
    {
        IAccessRule Parent { get; set; }

        string Description { get; set; }

        string KeyName { get; set; }
    }
}
