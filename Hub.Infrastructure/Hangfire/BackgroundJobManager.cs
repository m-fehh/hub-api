using Dapper;
using Hub.Infrastructure.Database;
using Hub.Infrastructure.Redis.Cache;
using System.Data.Common;

namespace Hub.Infrastructure.Hangfire
{
    public class BackgroundJobManager
    {
        private readonly DatabaseConnectionProvider databaseConnectionProvider;
        private readonly string schema;
        private readonly DbConnection conn;
        private readonly string environment;
        private readonly object locker = new object();
        public BackgroundJobManager(DatabaseConnectionProvider databaseConnectionProvider)
        {
            this.databaseConnectionProvider = databaseConnectionProvider;
            schema = this.databaseConnectionProvider.GetSchema();
            conn = databaseConnectionProvider.GetConnection();
            environment = Engine.AppSettings["environment"];
        }

        /// <summary>
        /// Verifica se a tarefa está habilitada no ambiente. A gestão é feita por uma tabela no banco de dados: {schema}.BackgroundJobManagment
        /// </summary>
        /// <param name="_jobName">Nome da tarefa a ser verificada</param>
        /// <param name="_orgId">O controle pode ser feito a nível de unidade, mas é opcional</param>
        /// <returns></returns>
        public bool IsActive(string _jobName, long? _orgId = null)
        {
            Func<string, long?, bool> fn = (jobName, orgId) =>
            {
                CreateManagmentTable();

                var query = $"SELECT b.IsActive FROM {schema}.BackgroundJobManagment b WHERE b.Environment = '{environment}' AND b.JobName = '{jobName}'";

                if (orgId != null)
                {
                    query += $" AND b.OrganizationalStructureId = {orgId}";
                }
                else
                {
                    query += $" AND b.OrganizationalStructureId IS NULL";
                }

                var isActive = conn.QueryFirstOrDefault<bool?>(query);

                if (isActive == null)
                {
                    lock (locker)
                    {
                        isActive = conn.QueryFirstOrDefault<bool?>(query);

                        if (isActive == null)
                        {
                            conn.Execute($"INSERT INTO {schema}.BackgroundJobManagment VALUES ('{environment}', '{jobName}', {(orgId != null ? orgId.ToString() : "NULL")}, 1)");

                            return true;
                        }
                    }
                }

                return isActive.Value;
            };

            return Engine.Resolve<CacheManager>().CacheAction(() => fn(_jobName, _orgId));
        }

        /// <summary>
        /// Cria a tabela de gerenciamento {schema}.BackgroundJobManagment
        /// </summary>
        private void CreateManagmentTable()
        {
            //para não depender do projeto svc-database, faz a gestão da criação da tabela de controle aqui mesmo
            var count = conn.QueryFirst<long>($"SELECT count(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schema}' AND TABLE_NAME = 'BackgroundJobManagment'");

            if (count == 0)
            {
                lock (locker)
                {
                    count = conn.QueryFirst<long>($"SELECT count(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schema}' AND TABLE_NAME = 'BackgroundJobManagment'");

                    if (count == 0)
                    {
                        conn.Execute(@$"CREATE TABLE {schema}.BackgroundJobManagment (
                        Environment VARCHAR(50) NOT NULL,
                        JobName VARCHAR(200) NOT NULL,
                        OrganizationalStructureId BIGINT,
                        IsActive INT NOT NULL)");
                    }
                }
            }

        }
    }
}
