namespace Hub.Shared.Interfaces
{
    public interface IHubUser : IBaseEntity
    {
        string Name { get; }
        string Login { get; }
        string Password { get; }
        ICollection<IAccessRule> AllRules { get; }
    }
}
