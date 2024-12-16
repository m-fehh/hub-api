namespace Hub.Shared.Interfaces
{
    public interface IPortalMenuItem : IBaseEntity
    {
        IPortalMenu Menu { get; set; }

        IPortalMenuItem Parent { get; set; }

        IAccessRule Rule { get; set; }

        string Url { get; set; }

        string IconName { get; set; }

        int Order { get; set; }
    }
}
