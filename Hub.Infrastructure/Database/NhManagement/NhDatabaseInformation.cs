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
        /// <summary>
        /// Connection String atual da aplicação
        /// </summary>
        string ConnectionString(string tenantName = null);

        /// <summary>
        /// Fornecedor do banco de dados atual da aplicação
        /// </summary>
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
                info = Singleton<NhConfigurationTenant>.Instance.Mapeamentos[0].ConfigurationTenants[0];
                //info = Singleton<NhConfigurationTenant>.Instance.Mapeamentos[0].ConfigurationTenants["default"];
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
                info = Singleton<NhConfigurationTenant>.Instance.Mapeamentos[0].ConfigurationTenants[0];
                //info = Singleton<NhConfigurationTenant>.Instance.Mapeamentos[0].ConfigurationTenants["default"];
            }

            if (info.ConnectionDriver.IndexOf("NHibernate.Driver.MicrosoftDataSqlClientDriver") >= 0 ||
                info.ConnectionDriver.IndexOf("SqlAzure") >= 0) return "sqlserver";

            if (info.ConnectionDriver.IndexOf("Oracle") >= 0) return "oracle";

            if (info.ConnectionDriver.IndexOf("MySql") >= 0) return "mysql";

            if (info.ConnectionDriver.IndexOf("SQLite") >= 0) return "sqlite";

            if (info.ConnectionDriver.IndexOf("SqlServerCe") >= 0) return "sqlceserver";

            throw new NotImplementedException();
        }
    }
}
