using Hub.API.Configuration.Context;
using Hub.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hub.API.Configuration.Filters
{
    public class SystemAuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var tenantName = Engine.Resolve<ITenantContext>().TenantName;

            if (!string.IsNullOrEmpty(tenantName) && tenantName?.ToLower() != "system")
            {
                // Retorna 403 Forbidden sem envolver autenticação
                context.Result = new ObjectResult(new { error = "Access denied" })
                {
                    StatusCode = 403
                };
            }
        }
    }
}
