using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Database.Interfaces;
using Hub.Shared.DataConfiguration;
using System.Configuration;

namespace Hub.Infrastructure.Database.NhManagement
{
    /// <summary>
    /// Classes que implementam essa interface fornece informações sobre o banco de dados atual da aplicação
    /// </summary>
    public interface IDatabaseInformation
    {
        // Connection String atual da aplicação
        string ConnectionString(string tenantName = null);

        // Fornecedor do banco de dados atual da aplicação
        string DatabaseSupplier(string tenantName = null);
    }

    internal class NhDatabaseInformation : IDatabaseInformation
    {
        public string ConnectionString(string tenantName = null)
        {
            NhConfigurationData info = null;

            if (tenantName == null) tenantName = Singleton<ISchemaNameProvider>.Instance.TenantName();

            foreach (NhConfigurationMapeamento item in Singleton<NhConfigurationTenant>.Instance.Mapeamentos)
            {
                info = item.ConfigurationTenants[tenantName];

                if (info == null)
                {
                    info = item.ConfigurationTenants[tenantName.ToLower()];
                }

                if (info != null) break;
            }

            if (info == null)
            {
                //info = Singleton<NhConfigurationTenant>.Instance.Mapeamentos[0].ConfigurationTenants[0];
                info = Singleton<NhConfigurationTenant>.Instance.Mapeamentos[0].ConfigurationTenants["default"];
            }

            var cs = info.ConnectionString;

            if (string.IsNullOrEmpty(cs)) cs = ConfigurationManager.ConnectionStrings[info.TenantId].ConnectionString;

            return cs;
        }

        public string DatabaseSupplier(string tenantName = null)
        {
            NhConfigurationData info = null;

            if (tenantName == null) tenantName = Singleton<ISchemaNameProvider>.Instance.TenantName();

            foreach (NhConfigurationMapeamento item in Singleton<NhConfigurationTenant>.Instance.Mapeamentos)
            {
                info = item.ConfigurationTenants[tenantName];

                if (info == null)
                {
                    info = item.ConfigurationTenants[tenantName.ToLower()];
                }

                if (info != null) break;
            }

            if (info == null)
            {
                //info = Singleton<NhConfigurationTenant>.Instance.Mapeamentos[0].ConfigurationTenants[0];
                info = Singleton<NhConfigurationTenant>.Instance.Mapeamentos[0].ConfigurationTenants["default"];
            }

            var driverMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "NHibernate.Driver.MicrosoftDataSqlClientDriver", "sqlserver" },
                { "SqlAzure", "sqlserver" },
                { "Oracle", "oracle" },
                { "MySql", "mysql" },
                { "SQLite", "sqlite" },
                { "SqlServerCe", "sqlceserver" }
            };

            foreach (var mapping in driverMappings)
            {
                if (info.ConnectionDriver.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return mapping.Value;
                }
            }

            throw new NotImplementedException();
        }
    }
}
