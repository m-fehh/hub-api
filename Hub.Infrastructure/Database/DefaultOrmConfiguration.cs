using Dapper;
using Hub.Domain.Entities.Admin;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Database.NhManagement;
using Microsoft.Data.SqlClient;

namespace Hub.Infrastructure.Database
{
    public interface IOrmConfiguration
    {
        void Configure();
    }

    /// <summary>
    /// configura a ferramenta ORM.
    /// Os parametros devem estar configurados nas appSettings da aplicação.
    /// </summary>
    public class DefaultOrmConfiguration : IOrmConfiguration
    {
        public void Configure()
        {
            var nhProf = Engine.AppSettings["ActiveNhProfiler"] != null ? bool.Parse(Engine.AppSettings["ActiveNhProfiler"]) : false;

            var mapeamentoNh = new NhConfigurationTenant
            {
                ActiveNhProfiler = nhProf,
                Mapeamentos = new NhConfigurationMapeamentoCollection()
            };

            var admCS = Engine.ConnectionString("adm");
            var csb = Engine.Resolve<ConnectionStringBaseConfigurator>().Get();

            if (!string.IsNullOrEmpty(admCS) && !string.IsNullOrEmpty(csb.ConnectionStringNhAssembly))
            {
                var map = new NhConfigurationMapeamento
                {
                    MapeamentoId = "default",
                    Assemblies = new NhAssemblyCollection(),
                    ConfigurationTenants = new NhConfigurationDataCollection()
                };

                var assemblies = csb.ConnectionStringNhAssembly.Split(';');

                foreach (var assembly in assemblies)
                {
                    map.Assemblies.Add(new NhAssembly
                    {
                        Name = assembly
                    });
                }

                // CREATE DATABASE TrainlyDB

                using (var connection = new SqlConnection(admCS))
                {
                    // Verifica se o schema 'adm' existe e cria se não existir
                    var checkSchemaQuery = @"
                        IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'adm')
                        BEGIN
                            EXEC('CREATE SCHEMA adm');
                        END";

                    connection.Execute(checkSchemaQuery);

                    // Verifica se a tabela 'Teams' existe no schema 'adm'
                    var checkTableQuery = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_SCHEMA = 'adm' AND TABLE_NAME = 'Teams'";

                    var tableExists = connection.ExecuteScalar<int>(checkTableQuery) > 0;

                    if (!tableExists)
                    {
                        var createTableQuery = @"
                        CREATE TABLE adm.Teams (
                            Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                            Name NVARCHAR(255),
                            Subdomain NVARCHAR(255),
                            Logo NVARCHAR(255),
                            GroupName NVARCHAR(255)
                        )";

                        connection.Execute(createTableQuery);

                        // Após a criação da tabela, cria um cliente fake
                        var defaultTenant = new Tenants
                        {
                            Name = "Trainly Base",
                            Subdomain = "base.trainly",
                        };

                        var insertQuery = "INSERT INTO adm.Teams (Name, Subdomain) VALUES (@Name, @Subdomain)";
                        connection.Execute(insertQuery, new { defaultTenant.Name, defaultTenant.Subdomain });

                        // Adiciona o fake client à lista
                        var clients = new List<Tenants> { defaultTenant };

                        // Processo de configuração continua com os dados do fakeClient
                        foreach (var client in clients)
                        {
                            using (Engine.BeginLifetimeScope(client.Subdomain))
                            {
                                using (Engine.BeginIgnoreTenantConfigs(false))
                                {
                                    var cs = Engine.ConnectionString("default");
                                    var csReadOnly = Engine.ConnectionString("readOnly");

                                    map.ConfigurationTenants.Add(new NhConfigurationData
                                    {
                                        TenantId = client.Subdomain,
                                        ConnectionString = cs,
                                        ConnectionProvider = "NHibernate.Connection.DriverConnectionProvider",
                                        ConnectionDriver = "NHibernate.Driver.MicrosoftDataSqlClientDriver",
                                        Dialect = "Hub.Infrastructure.Database.NhManagement.FMKSQLDIalect, Hub.Infrastructure",
                                        CurrentSessionContext = "async_local",
                                        UseSecondLevelCache = "false",
                                        UseQueryCache = "false",
                                        SchemaDefault = $"{csb.ConnectionStringBaseSchema}{client.Id}",
                                        CacheProvider = "CoreDistributedCacheProvider"
                                    });

                                    //Se houver uma connection string diferenciada para read only, então adiciona um item novo na coleção.
                                    //Isso fará com que o construtor de fábricas de sessões consiga pegar a connection string personalizada e criar essa nova conexão.
                                    //Agora, se for a mesma connection string (ou não existir) não adiciona nada que o provedor de fábricas vai se virar (NhSessionFactoryProvider)
                                    if (!string.IsNullOrEmpty(csReadOnly) || csReadOnly != cs)
                                    {
                                        map.ConfigurationTenants.Add(new NhConfigurationData
                                        {
                                            TenantId = $"{client.Subdomain}-readOnly",
                                            ConnectionString = csReadOnly,
                                            ConnectionProvider = "NHibernate.Connection.DriverConnectionProvider",
                                            ConnectionDriver = "NHibernate.Driver.MicrosoftDataSqlClientDriver",
                                            Dialect = "Hub.Infrastructure.Database.NhManagement.FMKSQLDIalect, Hub.Infrastructure",
                                            CurrentSessionContext = "async_local",
                                            UseSecondLevelCache = "false",
                                            UseQueryCache = "false",
                                            SchemaDefault = $"{csb.ConnectionStringBaseSchema}{client.Id}",
                                            CacheProvider = "CoreDistributedCacheProvider"
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Se a tabela já existir, faz a consulta normalmente
                        var query = "select Id, Name, Subdomain, Logo from adm.Teams";

                        var clients = connection.Query<Tenants>(query).ToList();

                        if (clients.Count == 0)
                        {
                            var defaultTenant = new Tenants
                            {
                                Name = "Trainly Base",
                                Subdomain = "base.trainly",
                            };

                            var insertQuery = "INSERT INTO adm.Teams (Name, Subdomain) VALUES (@Name, @Subdomain)";
                            connection.Execute(insertQuery, new { defaultTenant.Name, defaultTenant.Subdomain });

                            clients.Add(defaultTenant);
                        }

                        foreach (var client in clients)
                        {
                            using (Engine.BeginLifetimeScope(client.Subdomain))
                            {
                                using (Engine.BeginIgnoreTenantConfigs(false))
                                {
                                    var cs = Engine.ConnectionString("default");
                                    var csReadOnly = Engine.ConnectionString("readOnly");

                                    map.ConfigurationTenants.Add(new NhConfigurationData
                                    {
                                        TenantId = client.Subdomain,
                                        ConnectionString = cs,
                                        ConnectionProvider = "NHibernate.Connection.DriverConnectionProvider",
                                        ConnectionDriver = "NHibernate.Driver.MicrosoftDataSqlClientDriver",
                                        Dialect = "Hub.Infrastructure.Database.NhManagement.FMKSQLDIalect, Hub.Infrastructure",
                                        CurrentSessionContext = "async_local",
                                        UseSecondLevelCache = "false",
                                        UseQueryCache = "false",
                                        SchemaDefault = $"{csb.ConnectionStringBaseSchema}{client.Id}",
                                        CacheProvider = "CoreDistributedCacheProvider"
                                    });

                                    //Se houver uma connection string diferenciada para read only, então adiciona um item novo na coleção.
                                    //Isso fará com que o construtor de fábricas de sessões consiga pegar a connection string personalizada e criar essa nova conexão.
                                    //Agora, se for a mesma connection string (ou não existir) não adiciona nada que o provedor de fábricas vai se virar (NhSessionFactoryProvider)
                                    if (!string.IsNullOrEmpty(csReadOnly) || csReadOnly != cs)
                                    {
                                        map.ConfigurationTenants.Add(new NhConfigurationData
                                        {
                                            TenantId = $"{client.Subdomain}-readOnly",
                                            ConnectionString = csReadOnly,
                                            ConnectionProvider = "NHibernate.Connection.DriverConnectionProvider",
                                            ConnectionDriver = "NHibernate.Driver.MicrosoftDataSqlClientDriver",
                                            Dialect = "Hub.Infrastructure.Database.NhManagement.FMKSQLDIalect, Hub.Infrastructure",
                                            CurrentSessionContext = "async_local",
                                            UseSecondLevelCache = "false",
                                            UseQueryCache = "false",
                                            SchemaDefault = $"{csb.ConnectionStringBaseSchema}{client.Id}",
                                            CacheProvider = "CoreDistributedCacheProvider"
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                if (Singleton<NhConfigurationTenant>.Instance?.AppPath != null)
                {
                    //se o valor do AppPath já estiver preenchido, preserva.
                    //acontece que nas functions o appPath é setado diretamento no Startup, e rotinas de reconfiguração do ORM fazem passar por aqui novamente.
                    mapeamentoNh.AppPath = Singleton<NhConfigurationTenant>.Instance?.AppPath;
                }
                else
                {
                    mapeamentoNh.AppPath = AppDomain.CurrentDomain.BaseDirectory;
                }

                mapeamentoNh.Mapeamentos.Add(map);
            }

            Singleton<NhConfigurationTenant>.Instance = mapeamentoNh;
        }
    }
}
