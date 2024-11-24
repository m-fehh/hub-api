using Hub.Infrastructure.Database;
using System.Collections.Concurrent;

namespace Hub.API
{
    public class HubProvider : ISchemaNameProvider
    {
        public static AsyncLocal<string> CurrentTenant = new AsyncLocal<string>();

        public static ConcurrentDictionary<string, string> Subdomains = new ConcurrentDictionary<string, string>();

        private readonly IHttpContextAccessor _httpContextAccessor;

        public HubProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Obtém o nome do tenant a partir da requisição atual.
        /// </summary>
        /// <returns>Nome do tenant identificado ou null.</returns>
        public string TenantName()
        {
            return GetTenantFromRequest();
        }

        /// <summary>
        /// Obtém o tenant da aplicação a partir de um cabeçalho ou cookie.
        /// </summary>
        /// <returns>Nome do tenant identificado.</returns>
        public string GetTenantFromRequest()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
            {
                return null;
            }

            // Tenta obter o tenant do cabeçalho ou cookie
            var tenant = GetTenantFromHeader(context) ?? GetTenantFromCookie(context);

            if (tenant != null)
            {
                // Se o tenant foi encontrado, armazena no AsyncLocal para acesso assíncrono
                CurrentTenant.Value = tenant;
            }

            return tenant;
        }

        /// <summary>
        /// Obtém o tenant a partir do cabeçalho "X-Tenant-Id" da requisição HTTP.
        /// </summary>
        /// <param name="context">O contexto da requisição HTTP</param>
        /// <returns>O valor do tenant no cabeçalho ou null caso não exista.</returns>
        private string GetTenantFromHeader(HttpContext context)
        {
            // Verifica se o cabeçalho "X-Tenant-Id" existe e retorna seu valor
            if (context.Request.Headers.ContainsKey("X-Tenant-Id"))
            {
                return context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Obtém o tenant a partir do cookie "TenantId" da requisição HTTP.
        /// </summary>
        /// <param name="context">O contexto da requisição HTTP</param>
        /// <returns>O valor do tenant no cookie ou null caso não exista.</returns>
        private string GetTenantFromCookie(HttpContext context)
        {
            // Verifica se o cookie "TenantId" existe e retorna seu valor
            if (context.Request.Cookies.ContainsKey("TenantId"))
            {
                return context.Request.Cookies["TenantId"];
            }
            return null;
        }
    }
}
