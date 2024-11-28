namespace Hub.API.Configuration
{
    public interface ITenantContext
    {
        string TenantName { get; }
    }

    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string TenantName
        {
            get
            {
                return _httpContextAccessor.HttpContext?.Request.RouteValues["tenantName"]?.ToString();
            }
        }
    }
}
