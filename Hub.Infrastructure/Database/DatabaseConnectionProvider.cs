using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Extensions;
using Hub.Shared.Interfaces.MultiTenant;
using Microsoft.Extensions.Logging;
using Dapper.Logging.Configuration;
using System.Data.Common;
using System.Data;
using static Hub.Infrastructure.Database.NhManagement.NhSessionProvider;
using Microsoft.Data.SqlClient;
using Dapper.Logging;

namespace Hub.Infrastructure.Database
{
    /// <summary>
    /// Serviço que oferece uma conexão com o SQL server
    /// </summary>
    public class DatabaseConnectionProvider : IDisposable
    {
        private DbConnection connection = null;
        private DbConnection connectionRO = null;
        private readonly ConnectionStringBaseConfigurator connectionStringBaseConfigurator;

        public DatabaseConnectionProvider(ConnectionStringBaseConfigurator connectionStringBaseConfigurator)
        {
            this.connectionStringBaseConfigurator = connectionStringBaseConfigurator;
        }

        /// <summary>
        /// Retorna uma nova conexão com o SQL
        /// </summary>
        /// <param name="integrateWithNHibernate">Permite integrar a sessão com a conexão aberta pelo NHibernate</param>
        /// <returns></returns>
        public DbConnection GetConnection(bool integrateWithNHibernate = false)
        {
            if (integrateWithNHibernate)
            {
                var nhSession = NhSessionProvider.CurrentSession();

                if (nhSession != null)
                {
                    return nhSession.Connection as SqlConnection;
                }
            }

            if (connection != null)
            {
                return connection;
            }

            connection = CreateConnection(Engine.ConnectionString("default"));
            return connection;
        }

        public string GetSchema(Action<ConnectionStringBaseConfigurator> config = null)
        {
            if (config != null)
            {
                var connectionStringNewConfiguratior = new ConnectionStringBaseConfigurator();
                config.Invoke(connectionStringNewConfiguratior);

                return connectionStringNewConfiguratior.Get().ConnectionStringBaseSchema + Engine.Resolve<ITenantManager>().GetInfo().Id;
            }

            return connectionStringBaseConfigurator.Get().ConnectionStringBaseSchema + Engine.Resolve<ITenantManager>().GetInfo().Id;
        }

        public IDbTransaction GetTransaction()
        {
            var nhSession = NhSessionProvider.CurrentSession();

            if (nhSession != null)
            {
                using (var command = nhSession.Connection.CreateCommand())
                {
                    nhSession.Transaction.Enlist(command);
                    return command.Transaction;
                }
            }

            if (NhSessionProvider.SessionMode.Value == ESessionMode.ReadOnly)
            {
                return connectionRO.BeginTransaction();
            }
            else
            {
                return connection.BeginTransaction();
            }
        }

        public void Dispose()
        {
            if (connectionRO != null)
            {
                connectionRO.Dispose();
                connectionRO = null;
            }

            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        private DbConnection CreateConnection(string connectionString)
        {
            DbConnection NewConnection()
            {
                return new SqlConnection(connectionString);
            };

            if (Engine.AppSettings["PrintNhQueries"].TryParseBoolean(false))
            {
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                });

                var logger = loggerFactory.CreateLogger<ContextlessLoggingFactory>();

                var builder = new DbLoggingConfigurationBuilder();
                var factory = new ContextlessLoggingFactory(logger, builder.Build(), NewConnection);
                return factory.CreateConnection();
            }

            return NewConnection();
        }
    }
}
