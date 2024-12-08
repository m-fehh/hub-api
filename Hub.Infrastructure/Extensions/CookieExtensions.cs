using Hub.Infrastructure.Web;

namespace Hub.Infrastructure.Extensions
{
    public static class CookieExtensions
    {
        public static void CleanCookies()
        {
            HttpContextHelper.Current.Response.Cookies.Delete("Authentication");
            HttpContextHelper.Current.Response.Cookies.Delete("ASP.NET_SessionId");
        }
    }
}
