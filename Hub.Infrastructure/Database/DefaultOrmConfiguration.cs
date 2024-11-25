using Dapper;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Database.Models;
using Microsoft.Data.SqlClient;

namespace Hub.Infrastructure.Database
{
    public interface IOrmConfiguration
    {
        void Configure();
    }

    public class DefaultOrmConfiguration : IOrmConfiguration
    {
        public void Configure()
        {
            var nhProf = Engine.AppSettings["ActiveNhProfiler"] != null ? bool.Parse(Engine.AppSettings["ActiveNhProfiler"]) : false;

            var sqlConfigurationTenant = new SqlConfigurationTenant
            {
                ActiveProfiler = nhProf,
                Mapeamentos = new SqlConfigurationMapeamentoCollection()
            };

            var admCS = Engine.ConnectionString("adm");
            var csb = Engine.Resolve<ConnectionStringBaseConfigurator>().Get();

            if (!string.IsNullOrEmpty(admCS) && !string.IsNullOrEmpty(csb.ConnectionStringNhAssembly))
            {
                var map = new SqlConfigurationMapeamento
                {
                    MapeamentoId = "default",
                    Assemblies = new SqlAssemblyCollection(),
                    ConfigurationTenants = new SqlConfigurationDataCollection()
                };

                var assemblies = csb.ConnectionStringNhAssembly.Split(';');
                foreach (var assembly in assemblies)
                {
                    map.Assemblies.Add(new SqlAssembly { Name = assembly });
                }

                using (var connection = new SqlConnection(admCS))
                {
                    // Verifica se o schema 'adm' existe e cria se não existir
                    var checkSchemaQuery = @"
                        IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'adm')
                        BEGIN
                            EXEC('CREATE SCHEMA adm');
                        END";

                    connection.Execute(checkSchemaQuery);

                    // Verifica se a tabela 'Tenants' existe no schema 'adm'
                    var checkTableQuery = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_SCHEMA = 'adm' AND TABLE_NAME = 'Tenants'";

                    var tableExists = connection.ExecuteScalar<int>(checkTableQuery) > 0;

                    if (!tableExists)
                    {
                        var createTableQuery = @"
                        CREATE TABLE adm.Tenants (
                            Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                            Name NVARCHAR(255),
                            Subdomain NVARCHAR(255),
                            CultureName NVARCHAR(255)
                        )";
                        connection.Execute(createTableQuery);

                        var defaultTenant = new Tenants
                        {
                            Name = "Trainly Base",
                            Subdomain = "base.trainly",
                            CultureName = "pt-BR"
                        };

                        var insertQuery = "INSERT INTO adm.Tenants (Name, Subdomain, CultureName) VALUES (@Name, @Subdomain, @CultureName)";
                        connection.Execute(insertQuery, defaultTenant);

                        ConfigureTenants(map, new List<Tenants> { defaultTenant }, csb);
                    }
                    else
                    {
                        var query = "SELECT Id, Name, Subdomain, CultureName FROM adm.Tenants";
                        var clients = connection.Query<Tenants>(query).ToList();

                        if (!clients.Any())
                        {
                            var defaultTenant = new Tenants
                            {
                                Name = "Trainly Base",
                                Subdomain = "base.trainly",
                            };

                            var insertQuery = "INSERT INTO adm.Tenants (Name, Subdomain, CultureName) VALUES (@Name, @Subdomain, @CultureName)";
                            connection.Execute(insertQuery, defaultTenant);
                            clients.Add(defaultTenant);
                        }

                        ConfigureTenants(map, clients, csb);
                    }
                }

                // Verifica se a configuração de caminho da aplicação já foi definida
                if (Singleton<SqlConfigurationTenant>.Instance?.AppPath != null)
                {
                    sqlConfigurationTenant.AppPath = Singleton<SqlConfigurationTenant>.Instance.AppPath;
                }
                else
                {
                    sqlConfigurationTenant.AppPath = AppDomain.CurrentDomain.BaseDirectory;
                }

                sqlConfigurationTenant.Mapeamentos.Add(map);
            }

            // Atualiza a configuração global do singleton
            Singleton<SqlConfigurationTenant>.Instance = sqlConfigurationTenant;
        }

        private void ConfigureTenants(SqlConfigurationMapeamento map, List<Tenants> clients, ConnectionStringBaseVM csb)
        {
            foreach (var client in clients)
            {
                using (Engine.BeginLifetimeScope(client.Subdomain))
                {
                    using (Engine.BeginIgnoreTenantConfigs(false))
                    {
                        var cs = Engine.ConnectionString("default");
                        var csReadOnly = Engine.ConnectionString("readOnly");

                        map.ConfigurationTenants.Add(new SqlConfigurationData
                        {
                            TenantId = client.Subdomain,
                            ConnectionString = cs,
                            SchemaDefault = $"{csb.ConnectionStringBaseSchema}{client.Id}",
                        });

                        // Se houver uma connection string diferenciada para read-only, adiciona na coleção de tenants.
                        if (!string.IsNullOrEmpty(csReadOnly) && csReadOnly != cs)
                        {
                            map.ConfigurationTenants.Add(new SqlConfigurationData
                            {
                                TenantId = $"{client.Subdomain}-readOnly",
                                ConnectionString = csReadOnly,
                                SchemaDefault = $"{csb.ConnectionStringBaseSchema}{client.Id}",
                            });
                        }
                    }
                }
            }
        }
    }
}
