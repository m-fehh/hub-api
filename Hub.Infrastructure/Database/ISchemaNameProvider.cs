using Microsoft.AspNetCore.Http;

namespace Hub.Infrastructure.Database
{
    public interface ISchemaNameProvider
    {
        /// <summary>
        /// método responsável obter o tenant atual do sistema
        /// </summary>
        string TenantName();

        /// <summary>
        /// Método responsável por obter o tenant da aplicação a partir de um cookie ou cabeçalho.
        /// </summary>
        /// <returns>Tenant identificado</returns>
        string GetTenantFromRequest();
    }

    public class SchemaNameProvider : ISchemaNameProvider
    {
        private static readonly HttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        /// <summary>
        /// Obtém o nome do tenant a partir da requisição atual.
        /// Este método chama o método GetTenantFromRequest para capturar o tenant da requisição.
        /// </summary>
        /// <returns>O nome do tenant, se encontrado; caso contrário, null.</returns>
        public string TenantName()
        {
            return GetTenantFromRequest();
        }

        /// <summary>
        /// Obtém o tenant da requisição HTTP atual.
        /// Ele busca o tenant a partir do cabeçalho ou cookie da requisição.
        /// </summary>
        /// <returns>O nome do tenant, se encontrado; caso contrário, null.</returns>
        public string GetTenantFromRequest()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                return null;
            }

            var tenant = GetTenantFromHeader(context) ?? GetTenantFromCookie(context);
            return tenant;
        }

        /// <summary>
        /// Obtém o tenant a partir do cabeçalho "X-Tenant-Id" da requisição HTTP.
        /// </summary>
        /// <param name="context">O contexto da requisição HTTP</param>
        /// <returns>O valor do tenant no cabeçalho ou null caso não exista.</returns>
        private string GetTenantFromHeader(HttpContext context)
        {
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
            if (context.Request.Cookies.ContainsKey("TenantId"))
            {
                return context.Request.Cookies["TenantId"];
            }
            return null;
        }
    }
}
