namespace Hub.Infrastructure.Exceptions
{
    /// <summary>
    /// Lançada quando comando Engine.BeginLifetimeScope não for executado anteriormente
    /// </summary>
    public class UndefinedTenantException : Exception
    {
    }
}
