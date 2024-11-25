using Hub.Infrastructure;
using Hub.Infrastructure.Database.Interfaces;
using Hub.Infrastructure.MultiTenant;
using Microsoft.AspNetCore.Http;

namespace Hub.API.Configuration
{
    public class HubProvider : ISchemaNameProvider
    {
        private static readonly HttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        public string TenantName()
        {
            // Tenta resolver o tenant fixo se já estiver configurado
            var fixedDomain = Engine.Resolve<TenantLifeTimeScope>().CurrentTenantName;
            if (!string.IsNullOrEmpty(fixedDomain)) return fixedDomain;

            // Se não, tenta resolver a partir do subdomínio
            var tenantNameFromSubdomain = GetTenantFromSubdomain();
            if (!string.IsNullOrEmpty(tenantNameFromSubdomain)) return tenantNameFromSubdomain;

            // Se não conseguir, retorna um nome de tenant padrão
            return "default";
        }

        // Método para pegar o nome do tenant a partir do subdomínio
        public string GetTenantFromSubdomain()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Request?.Host != null)
            {
                var host = context.Request.Host.Value;
                var subdomain = host.Split('.')[0]; // Assume que o primeiro subdomínio é o nome do tenant
                return subdomain; // Pode retornar o subdomínio, ou manipular conforme necessário
            }

            return null;
        }
    }
}
