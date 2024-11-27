using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Database.Interfaces;
using Hub.Shared.Interfaces.MultiTenant;

namespace Hub.Infrastructure.MultiTenant
{
    /// <summary>
    /// Classe para obtenção das informações do atual tenant da aplicação.
    /// </summary>
    public class DefaultTenantManager : ITenantManager
    {
        public ITenantInfo GetInfo()
        {
            var _tenantName = Singleton<ISchemaNameProvider>.Instance.TenantName();

            Func<string, Object> fn = (tenantName) =>
            {
                return new TenantInfo()
                {
                    Id = 1, // Id do tenant
                    Name = "Trainly Base", // Nome do tenant
                    Subdomain = "base.trainly", // Subdomínio do tenant
                    CultureName = "en-US" // Cultura do tenant
                };
            };

            // Chamada direta da função fn
            var clientInfo = fn(_tenantName);
            return (ITenantInfo)clientInfo;
        }

    }
}
