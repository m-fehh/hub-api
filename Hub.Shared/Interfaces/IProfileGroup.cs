namespace Hub.Shared.Interfaces
{
    public interface IProfileGroup : IBaseEntity, ICloneable
    {
        string Name { get; set; }

        ICollection<IAccessRule> Rules { get; set; }

        bool Administrator { get; set; }
    }
}
