using Hub.Infrastructure.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


namespace Hub.Infrastructure.Autofac
{
    public interface ISchemaNameProvider
    {
        /// <summary>
        /// método responsável obter o tenant atual do sistema
        /// </summary>
        string TenantName();
    }

    public class SchemaNameProvider : ISchemaNameProvider
    {
        private static readonly HttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        public string TenantName()
        {
            // Retorna o TenantName se já estiver configurado
            string currentTenantName = Engine.Resolve<TenantLifeTimeScope>().CurrentTenantName;
            if (!string.IsNullOrEmpty(currentTenantName))
            {
                return currentTenantName;
            }

            // Verifica se HttpContext está disponível
            if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Request == null)
            {
                return "system";
            }

            // Recupera RouteValues do HttpContext
            var routeValues = _httpContextAccessor.HttpContext.GetRouteData()?.Values;

            // Busca tenantName nos RouteValues
            if (routeValues != null && routeValues.TryGetValue("tenantName", out var tenantName) && tenantName != null)
            {
                return tenantName.ToString();
            }


            return "system";
        }
    }
}
