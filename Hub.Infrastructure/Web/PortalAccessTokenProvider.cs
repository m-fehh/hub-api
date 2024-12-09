using Hub.Infrastructure.Extensions;

namespace Hub.Infrastructure.Web
{
    public class PortalAccessTokenProvider : AccessTokenProvider
    {
        public override void ValidateTokenStatus(AccessTokenResult tokenResult)
        {
            if (tokenResult.Status != AccessTokenStatus.Valid)
            {
                CookieExtensions.CleanCookies();
            }
        }
    }
}
