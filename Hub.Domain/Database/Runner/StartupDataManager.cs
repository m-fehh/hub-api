using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure;
using Hub.Shared.Log;
using NHibernate;

namespace Hub.Domain.Database.Runner
{
    /// <summary>
    /// Todas as classes que implementarem essa interface serão executadas assim que o banco de dados estiver pronto para ser semeado (inserção de dados iniciais).
    /// </summary>
    public interface IStartupData
    {
        void Execute();

        long Order { get; }
    }

    public static class StartupDataManager
    {
        private static readonly object _locker = new object();
        private static List<string> executed = new List<string>();

        /// <summary>
        /// Irá procurar por todos os scripts de inicialização que insere dados no banco (<see cref="IStartupData"/>) e executá-los na ordem (apenas caso o flag <see cref="ReadyToExecute"/> esteja marcado como true).
        /// </summary>
        public static void ExecuteAll()
        {
            lock (_locker)
            {
                var tenant = Singleton<ISchemaNameProvider>.Instance.TenantName();

                if (!StartupDataManager.executed.Contains(tenant))
                {
                    var type = typeof(IStartupData);

                    var startUpDataTypes = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetTypes())
                        .Where(p => type.IsAssignableFrom(p) && p.IsInterface == false);

                    var startUpDatas = new List<IStartupData>();

                    var startUpDataRepository = Engine.Resolve<IRepository<StartUpDataExecuted>>();

                    var startupsExecuted = startUpDataRepository.Table.Select(a => a.Name).ToList();

                    foreach (var startUpDataType in startUpDataTypes.Where(b => !startupsExecuted.Contains(b.Name)))
                    {
                        startUpDatas.Add((IStartupData)Activator.CreateInstance(startUpDataType));
                    }

                    startUpDatas = startUpDatas.AsQueryable().OrderBy(st => st.Order).ToList();

                    if (startUpDatas.Count > 0)
                    {
                        using (Engine.BeginLifetimeScope())
                        {
                            using (Engine.Resolve<IgnoreLogScope>().BeginIgnore())
                            {
                                foreach (var startUpData in startUpDatas)
                                {
                                    using (startUpDataRepository.BeginTransaction())
                                    {
                                        startUpData.Execute();

                                        var startUpDataExecuted = new StartUpDataExecuted();
                                        startUpDataExecuted.Name = startUpData.GetType().Name;
                                        startUpDataExecuted.Date = DateTime.Now;

                                        startUpDataRepository.Insert(startUpDataExecuted);

                                        startUpDataRepository.Commit();

                                        startUpDataRepository.Clear();
                                    }
                                }
                            }
                        }
                    }

                    StartupDataManager.executed.Add(tenant);
                }
            }
        }

        public static bool SchemaExists(string schema)
        {
            var repository = Engine.Resolve<IRepository>();

            var schemaName = (IEnumerable<object>)repository.CreateSQLQuery($"SELECT name from sys.schemas where name = '{schema}'").AddScalar("name", NHibernateUtil.String).List();

            return schemaName.Any();
        }
    }
}
