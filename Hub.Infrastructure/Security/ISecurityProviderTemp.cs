namespace Hub.Infrastructure.Security
{
    public interface ISecurityProviderTemp : ISecurityProvider
    {
        bool AuthenticateTemp(string userName, string tempPassword);
    }
}
