using Dapper;
using Hub.Infrastructure.Autofac;
using Hub.Shared.DataConfiguration.Admin;
using Hub.Shared.Interfaces.MultiTenant;
using Microsoft.Data.SqlClient;

namespace Hub.Infrastructure.MultiTenant
{
    /// <summary>
    /// Classe para obtenção das informações do atual tenant da aplicação.
    /// </summary>
    public class DefaultTenantManager : ITenantManager
    {
        public ITenantInfo GetInfo()
        {
            var tenantName = Singleton<ISchemaNameProvider>.Instance.TenantName();

            using (var connection = new SqlConnection(Engine.AppSettings["ConnectionString-adm"]))
            {
                var client = connection.QueryFirstOrDefault<Tenants>("SELECT Id, Name, Subdomain, IsActive, CultureName FROM adm.tenants WHERE Subdomain = @tenantName", new { tenantName });

                return client;
            }
        }
    }
}

