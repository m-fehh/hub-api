namespace Hub.Shared.Interfaces
{
    public interface IPortalMenu : IBaseEntity
    {
        string Name { get; set; }

        IList<IPortalMenuItem> Itens { get; set; }
    }
}
