namespace Hub.Shared.Interfaces.Logger
{
    /// <summary>
    /// As entidades que extenderem essa interface estarão sujeitas a gravação de log
    /// </summary>
    public interface ILogableEntity : IBaseEntity
    {
    }

    public interface ILogableEntityCustomName : ILogableEntity
    {
        string CustomLogName { get; set; }
    }
}
