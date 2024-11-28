using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Database.Interfaces;
using Hub.Infrastructure.Database.NhManagement.Migrations;
using Hub.Shared.DataConfiguration;
using NHibernate;
using NHibernate.Context;
using System.Collections.Concurrent;

namespace Hub.Infrastructure.Database.NhManagement
{
    internal static class NhSessionProvider
    {
        static NhSessionProvider()
        {
            SessionMode.Value = ESessionMode.Normal;
        }

        public enum ESessionMode
        {
            Normal = 1,
            ReadOnly = 2
        }

        #region Private Data

        private static readonly object _locker = new object();

        private static List<string> tenantExecuted = new List<string>();

        private static ConcurrentDictionary<string, ISessionFactory> _sessionFactories;

        private static bool _profilerInicialized;

        private static object lockerForStartup = new object();

        #endregion

        public static AsyncLocal<IStatelessSession> StatelessSession = new AsyncLocal<IStatelessSession>();

        public static AsyncLocal<ISession> ReadOnlySession = new AsyncLocal<ISession>();

        public static AsyncLocal<ESessionMode> SessionMode = new AsyncLocal<ESessionMode>();



        /// <summary>
        /// Retorna a sessão do NHibernate atual do usuário. Se não existir, criará uma nova e a armazenará num dicionário de sessões.
        /// </summary>
        public static ISession CurrentSession(string tenantName = "")
        {
            if (!_profilerInicialized && Singleton<NhConfigurationTenant>.Instance != null && Singleton<NhConfigurationTenant>.Instance.ActiveNhProfiler)
            {
                HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();

                _profilerInicialized = true;
            }

            ISessionFactory factory = GetSessionFactory(tenantName);

            if (!CurrentSessionContext.HasBind(factory))
            {
                ISession session = NewSession(factory);

                CurrentSessionContext.Bind(session);

                NhGlobalData.CloseCurrentSession = CloseCurrentSession;

                NhGlobalData.CloseCurrentFactory = CloseCurrentFactory;

                return session;
            }
            else
            {
                return factory.GetCurrentSession();
            }
        }

        /// <summary>
        /// Fecha a sessão do NHibernate atual do usuário.
        /// </summary>
        public static void CloseCurrentSession()
        {
            ISessionFactory factory = GetSessionFactory(createIfNotExists: false);

            if (factory == null) return;

            if (CurrentSessionContext.HasBind(factory))
            {
                factory.GetCurrentSession().Clear();
            }
        }

        public static void CloseCurrentFactory()
        {
            lock (lockerForStartup)
            {
                var name = Singleton<ISchemaNameProvider>.Instance.TenantName();

                ISessionFactory factory;

                if (_sessionFactories.ContainsKey(name))
                {
                    if (_sessionFactories.TryRemove(name, out factory))
                    {
                        if (factory == null) return;

                        factory.Close();


                    }
                }
            }
        }

        /// <summary>
        /// Retorna a fábrica de sessões corrente ou configura uma nova caso não exista.
        /// </summary>
        internal static ISessionFactory GetSessionFactory(string tenantName = "", bool createIfNotExists = true)
        {
            if (_sessionFactories == null) _sessionFactories = new ConcurrentDictionary<string, ISessionFactory>();

            string name;

            if (string.IsNullOrEmpty(tenantName))
            {
                name = Singleton<ISchemaNameProvider>.Instance.TenantName();
            }
            else
            {
                name = tenantName;
            }

            if (!_sessionFactories.ContainsKey(name))
            {
                lock (lockerForStartup)
                {
                    //verifica novamente, pois a thread pode ter ficado aguardando no lock e outra thread pode ter inserido
                    if (!_sessionFactories.ContainsKey(name))
                    {
                        if (!createIfNotExists) return null;

                        var trys = 0;

                        using (ISessionFactory factory = NewSessionFactory(name))
                        {

                            while (!_sessionFactories.TryAdd(name, factory))
                            {
                                if (_sessionFactories.ContainsKey(name)) break;

                                trys++;

                                if (trys > 300)
                                {
                                    throw new Exception("Não foi possível criar a fábrica de sessão por excesso de tentativas.");
                                }

                                //serão 300 tentativas aguardando 100 milisegundos cada, isso representa 30 segundos de tentativas
                                Thread.Sleep(100);
                            }

                            IMigrationRunner migrationRunner = null;

                            if (Engine.TryResolve<IMigrationRunner>(out migrationRunner))
                            {
                                lock (_locker)
                                {
                                    if (!tenantExecuted.Contains(name))
                                    {
                                        tenantExecuted.Add(name);
                                        migrationRunner.MigrateToLatest();
                                    }
                                }
                            }
                        }
                    }
                }

            }
            else if (_sessionFactories[name] == null)
            {
                _sessionFactories[name] = NewSessionFactory(name);
            }
            else if (_sessionFactories[name].IsClosed)
            {
                _sessionFactories[name] = NewSessionFactory(name);
            }

            return _sessionFactories[name];
        }

        /// <summary>
        /// Criará uma nova sessão a partir da fábrica de sessões
        /// </summary>
        internal static ISession NewSession(ISessionFactory factory = null)
        {
            if (factory == null) factory = GetSessionFactory();

            ISession session = factory.OpenSession(new QueryHintInterceptor());

            session.FlushMode = FlushMode.Commit;

            return session;
        }

        public static IStatelessSession NewStatelessSession(ISessionFactory factory = null)
        {
            if (factory == null) factory = GetSessionFactory();

            return factory.OpenStatelessSession();
        }

        public static ISession CurrentLifetimeScopeSession
        {
            get
            {
                return ((NhLifetimeScopeSession)Engine.Resolve<ILifetimeScopeSession>()).GetSession();
            }
        }

        /// <summary>
        /// Criará uma nova fábrica de sessões a partir das configurações da aplicação
        /// </summary>
        /// <returns></returns>
        public static ISessionFactory NewSessionFactory(string tenantName)
        {
            NhConfigurationMapeamento mapeamento = null;

            NhConfigurationData info = null;

            var fnGetTenantInfo = (string tname) =>
            {
                NhConfigurationData returnInfo = null;

                var tenants = Singleton<NhConfigurationTenant>.Instance;

                var skip = false;

                while (returnInfo == null && skip == false)
                {
                    foreach (NhConfigurationMapeamento item in tenants.Mapeamentos)
                    {
                        returnInfo = item.ConfigurationTenants[tname];

                        if (returnInfo != null)
                        {
                            mapeamento = item;
                            skip = true;
                            break;
                        }
                    }

                    //if (returnInfo == null)
                    //{
                    //    tname = tname.Replace("-readOnly", "");
                    //}
                    //else
                    //{
                    //    skip = true;
                    //}

                    skip = true;
                }

                return returnInfo;
            };

            info = fnGetTenantInfo(tenantName);

            if (info == null)
            {
                if (Engine.Resolve<INhStartSessionFactory>().PopulateTenantCollection != null)
                {
                    Engine.Resolve<INhStartSessionFactory>().PopulateTenantCollection();

                    info = fnGetTenantInfo(tenantName);
                }
            }

            if (info == null)
            {
                if (Engine.TryResolve<IOrmConfiguration>(out var ormConfiguration))
                {
                    Task.Factory.StartNew(() => ormConfiguration.Configure()).Wait();

                    info = fnGetTenantInfo(tenantName);
                }
            }

            if (info == null)
            {
                mapeamento = Singleton<NhConfigurationTenant>.Instance.Mapeamentos[0];
                info = mapeamento.ConfigurationTenants["default"];
            }

            var cfg = NhConfiguration.Get(info, mapeamento);

            return cfg.BuildSessionFactory();
        }
    }
}
