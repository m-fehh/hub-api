using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Database.Interfaces;

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

    public interface ITenantManager
    {
        ITenantInfo GetInfo();
    }

    public interface ITenantInfo
    {
        long Id { get; set; }

        string Name { get; set; }

        string Subdomain { get; set; }

        string CultureName { get; set; }
    }

    /// <summary>
    /// Implementação concreta de ITenantInfo.
    /// </summary>
    public class TenantInfo : ITenantInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Subdomain { get; set; }
        public string CultureName { get; set; }
    }
}
