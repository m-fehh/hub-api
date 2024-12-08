using Hub.Shared.Interfaces;
using Hub.Shared.Models.VMs;

namespace Hub.Infrastructure.Security
{
    /// A classe que implementar essa interface deverá fornecer um meio de autenticar e autorizar os usuários da aplicação.
    /// </summary>
    public interface ISecurityProvider
    {
        bool Authenticate(AuthenticationVM authenticationVM);

        void Authenticate(string token);

        bool Authorize(string role);

        /// <summary>
        /// Return a list containing only authorized roles
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        List<string> GetAuthorizedRoles(List<string> roles);

        IUser GetCurrent();

        void SetCurrentUser(IUser user);

        IProfileGroup GetCurrentProfile();

        long? GetCurrentId();

        long? GetCurrentProfileId();

        void AuthenticateByFormsAuthentication(string token);
    }
}
