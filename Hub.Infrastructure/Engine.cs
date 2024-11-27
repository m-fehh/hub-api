using Autofac;
using Autofac.Core;
using AutoMapper;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Hub.Infrastructure.Database;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Autofac.Dependency;
using Hub.Infrastructure.Mapper;
using Hub.Infrastructure.Tasks;
using Hub.Infrastructure.Helpers;
using Hub.Infrastructure.MultiTenant;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure.Localization;
using Hub.Infrastructure.Database.Interfaces;
using Hub.Shared.Interfaces.MultiTenant;
using Hub.Infrastructure.Database.NhManagement;

namespace Hub.Infrastructure
{
    public static class Engine
    {
        public static IContainer Container { get; set; }
        public static Assembly ExecutingAssembly { get; private set; }
        public static AsyncLocal<ILifetimeScope> CurrentScope = new AsyncLocal<ILifetimeScope>();
        private static AsyncLocal<LifetimeScopeDispose> currentScopeDisposer = new AsyncLocal<LifetimeScopeDispose>();
        public static AsyncLocal<bool> IgnoreTenantConfigsScope = new AsyncLocal<bool>();
        private static ContainerManager _containerManager;
        private static ILocalizationProvider _localizationProvider;
        private static List<IAutoMapperStartup> _autoMapperStartups;
        private static object appSettingsLock = new object();
        private static Action initializeAction = null;

        class LifetimeScopeDispose : IDisposable
        {
            public ILifetimeScope Scope { get; set; }

            public bool IsDisposed { get; set; }

            public LifetimeScopeDispose(ILifetimeScope scope)
            {
                Scope = scope;
                IsDisposed = false;
            }

            public void Dispose()
            {
                if (CurrentScope.Value == Scope)
                {
                    CurrentScope.Value.Dispose();
                    CurrentScope.Value = null;
                }

                IsDisposed = true;
            }
        }

        class IgnoreTenantConfigScopeDisposable : IDisposable
        {
            private bool originalValue;

            public IgnoreTenantConfigScopeDisposable()
            {
                originalValue = IgnoreTenantConfigsScope.Value;
                IgnoreTenantConfigsScope.Value = true;
            }

            public IgnoreTenantConfigScopeDisposable(bool ignoreValue)
            {
                originalValue = IgnoreTenantConfigsScope.Value;
                IgnoreTenantConfigsScope.Value = ignoreValue;
            }

            public void Dispose()
            {
                IgnoreTenantConfigsScope.Value = originalValue;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initialize(Assembly executingAssembly, ISchemaNameProvider nameProvider = null, IList<IStartupTask> tasks = null, IList<IDependencySetup> dependencyRegistrars = null, ConnectionStringBaseVM csb = null, ContainerBuilder containerBuilder = null, bool startListenerServiceBusAppSettingsReleaser = true)
        {
            ExecutingAssembly = executingAssembly;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            if (nameProvider == null)
            {
                nameProvider = new SchemaNameProvider();
            }

            Singleton<ISchemaNameProvider>.Instance = nameProvider;

            if (dependencyRegistrars == null)
            {
                dependencyRegistrars = new List<IDependencySetup>();
            }

            dependencyRegistrars.Add(new DependencySetup());

            _containerManager = new ContainerManager(dependencyRegistrars, containerBuilder);

            initializeAction = new Action(() =>
            {
                if (tasks == null)
                {
                    tasks = new List<IStartupTask>();
                }

                tasks.Add(new StartupTask());

                // Execute startup tasks
                tasks.OrderBy(t => t.Order).ToList().ForEach(x => x.Execute());

                if (_autoMapperStartups?.Count > 0)
                {
                    var config = new MapperConfiguration(cfg =>
                    {
                        Engine.RunAutoMapperStartups(cfg);
                    });

                    Singleton<IMapper>.Instance = config.CreateMapper();
                }

                TryResolve<ILocalizationProvider>(out _localizationProvider);

                if (csb != null)
                {
                    Engine.Resolve<ConnectionStringBaseConfigurator>().Set(csb);
                }

                IOrmConfiguration ormConfiguration = null;

                if (TryResolve(out ormConfiguration))
                {
                    ormConfiguration.Configure();
                }
            });

            if (_containerManager.Container != null)
            {
                initializeAction();
            }
        }

        /// <summary>
        /// Inicia um novo escopo para que o sistema tenha uma sessão stateless aberta para athread atual
        /// </summary>
        /// <returns></returns>
        public static IDisposable BeginStatelessSessionScope()
        {
            return Resolve<INhStatelessSessionScope>();
        }

        /// <summary>
        /// Permite conectar em um banco de dados réplica (somente leitura), se houver.
        /// Nenhuma operação de escrita poderá ser efetuada dentro desse escopo (o banco de dados não permite)
        /// </summary>
        /// <returns></returns>
        public static IDisposable BeginReadOnlySessionScope()
        {
            return Resolve<INhReadOnlySessionScope>().Start();
        }

        public static IDisposable BeginIgnoreTenantConfigs(bool ignoreTenantConfigs = true)
        {
            return new IgnoreTenantConfigScopeDisposable(ignoreTenantConfigs);
        }

        public static string ConnectionString(string settingName)
        {
            var cs = ConfigurationManager.ConnectionStrings[settingName];

            if (cs != null)
                return cs.ConnectionString;
            else
            {
                var key = AppSettings[$"ConnectionString-{settingName}"];

                if (key != null)
                {
                    return key;
                }

                return Environment.GetEnvironmentVariable($"ConnectionString-{settingName}");
            }
        }

        public static void RunAutoMapperStartups(IMapperConfigurationExpression cfg)
        {
            if (_autoMapperStartups == null) return;

            foreach (var item in _autoMapperStartups)
            {
                item.RegisterMaps(cfg);
            }
        }

        public static void SetContainer(IContainer container)
        {
            _containerManager.Container = container;

            initializeAction();
        }

        /// <summary>
        /// Permite iniciar uma nova thread para executar uma task, copiando o tenant atual e iniciando um novo escopo do IOC
        /// </summary>
        /// <param name="action">Ação a ser executada na nova task (cuidado, por criarmos um novo escopo do IOC, o ideal é instanciar seus serviços de novo dentro da action, procure não compartilhar serviços criados fora da action (pois o IOC está atrelado a thread de fora))</param>
        /// <returns></returns>
        public static Task NewTask(Func<Task> action)
        {
            return Task.Run(async () =>
            {
                using (BeginLifetimeScope(true))
                {
                    await action();
                }
            });
        }

        /// <summary>
        /// Inicia um novo ciclo de vida do injetor de dependências
        /// </summary>
        /// <param name="copyTenantName">define se irá executar o comando que passa o nome do tenant atual para o novo ciclo criado</param>
        /// <returns></returns>
        public static IDisposable BeginLifetimeScope(bool copyTenantName = false)
        {
            if (currentScopeDisposer.Value == null || currentScopeDisposer.Value.IsDisposed)
            {
                string tenantName = null;

                if (copyTenantName)
                {
                    tenantName = Singleton<ISchemaNameProvider>.Instance.TenantName();
                }

                CurrentScope.Value = ContainerManager.Container.BeginLifetimeScope();

                currentScopeDisposer.Value = new LifetimeScopeDispose(CurrentScope.Value);

                if (copyTenantName)
                {
                    Resolve<TenantLifeTimeScope>().Start(tenantName);
                }

                return currentScopeDisposer.Value;
            }

            return null;
        }

        /// <summary>
        /// Inicia um novo ciclo de vida do injetor de dependências
        /// </summary>
        /// <param name="tenantName">nome do tenant para o ciclo de vida que será criado</param>
        /// <param name="forceTenantAndCulture"></param>
        /// <returns></returns>
        public static IDisposable BeginLifetimeScope(string tenantName, bool forceTenantAndCulture = false)
        {
            if (currentScopeDisposer.Value == null || currentScopeDisposer.Value.IsDisposed)
            {
                CurrentScope.Value = ContainerManager.Container.BeginLifetimeScope();

                currentScopeDisposer.Value = new LifetimeScopeDispose(CurrentScope.Value);

                var tenantLifeTimeScope = Resolve<TenantLifeTimeScope>();

                if (forceTenantAndCulture)
                {
                    var info = Resolve<ITenantManager>().GetInfo();
                    if (info != null)
                    {
                        if (info.CultureName != null)
                        {
                            var ci = new CultureInfo(CultureInfoProvider.SetCultureInfo(info.CultureName));
                            CultureInfo.CurrentCulture = ci;
                            CultureInfo.CurrentUICulture = ci;
                        }

                        tenantLifeTimeScope.Start(info.Subdomain);
                    }
                    else
                    {
                        tenantLifeTimeScope.Start(tenantName);
                    }
                }
                else
                {
                    tenantLifeTimeScope.Start(tenantName);
                }

                //se não houver culture definida, define a padrão (situação ocorreu no jobs em ambientes linux/docker)
                if (string.IsNullOrEmpty(CultureInfo.CurrentCulture?.Name))
                {
                    CultureInfoProvider.SetDefaultCultureInfo();
                }

                return currentScopeDisposer.Value;
            }

            return null;
        }

        public static T Resolve<T>()
        {
            return ContainerManager.Resolve<T>("", CurrentScope.Value);
        }

        public static T Resolve<T>(ILifetimeScope scope)
        {
            return ContainerManager.Resolve<T>("", scope);
        }

        public static T Resolve<T>(Dictionary<string, object> parameters)
        {
            var listParameters = new List<Parameter>();

            foreach (var item in parameters)
            {
                listParameters.Add(new NamedParameter(item.Key, item.Value));
            }

            return ContainerManager.Container.Resolve<T>(listParameters);
        }

        public static object Resolve(Type type)
        {
            return ContainerManager.Resolve(type);
        }

        public static bool TryResolve<T>(out T instance) where T : class
        {
            return ContainerManager.Container.TryResolve(out instance);
        }

        public static bool TryResolve(Type type, out object instance)
        {
            return ContainerManager.Container.TryResolve(type, out instance);
        }

        public static object Resolve(Type type, params Type[] typeArguments)
        {
            return ContainerManager.Resolve(type.MakeGenericType(typeArguments));
        }

        public static bool TryResolve(Type type, out object instance, params Type[] typeArguments)
        {
            try
            {
                instance = ContainerManager.Resolve(type.MakeGenericType(typeArguments));

                return true;
            }
            catch (Exception)
            {
                instance = null;

                return false;
            }
        }

        public static T[] ResolveAll<T>()
        {
            return ContainerManager.ResolveAll<T>();
        }

        public static string Get(string key)
        {
            if (_localizationProvider == null) return key;

            return _localizationProvider.Get(key);
        }

        public static string GetByValue(string value)
        {
            if (_localizationProvider == null) return value;

            return _localizationProvider.GetByValue(value);
        }

        public static string Get(string key, CultureInfo culture)
        {
            if (_localizationProvider == null) return key;

            return _localizationProvider.Get(key, culture);
        }

        public static string Get(string key, params object[] args)
        {
            if (_localizationProvider == null) return key;

            return string.Format(_localizationProvider.Get(key), args);
        }

        public static string Get(string key, CultureInfo culture, params object[] args)
        {
            if (_localizationProvider == null) return key;

            return string.Format(_localizationProvider.Get(key, culture), args);
        }

        public static ContainerManager ContainerManager
        {
            get { return _containerManager; }
        }


        static NameValueCollection ConfigCollection = null;

        static Dictionary<string, NameValueCollection> TenantConfigCollection = new Dictionary<string, NameValueCollection>();

        static AsyncLocal<NameValueCollection> AsyncLocalConfigCollection = new AsyncLocal<NameValueCollection>();
        public static NameValueCollection AppSettings
        {
            get
            {
                if (ConfigCollection == null)
                {
                    lock (appSettingsLock)
                    {
                        if (ConfigCollection == null)
                        {
                            NameValueCollection all;

                            if (ConfigurationManager.AppSettings.Keys.Count != 0)
                            {
                                all = new NameValueCollection(ConfigurationManager.AppSettings);
                            }
                            else
                            {
                                all = new NameValueCollection(Environment.GetEnvironmentVariables().ToNameValueCollection());
                            }

                            // Aplicar chaves de debug (substituir qualquer chave quando em modo Debug)
                            // Exemplo de chave no appsettings.json: "Debug:elos-api-endpoint"
                            foreach (var debugKey in all.AllKeys.Where(a => a.StartsWith("Debug:")))
                            {
                                var originalKey = debugKey.Replace("Debug:", "");

                                if (all.AllKeys.Contains(originalKey))
                                {
                                    all[originalKey] = all[debugKey];
                                }
                                else
                                {
                                    all.Add(originalKey, all[debugKey]);
                                }
                            }

                            ConfigCollection = all;
                        }
                    }
                }

                string tenantName = "";

                if (ContainerManager?.Container != null && IgnoreTenantConfigsScope.Value == false)
                {
                    tenantName = Singleton<ISchemaNameProvider>.Instance.TenantName();

                    //if (tenantName.Equals("adm", StringComparison.OrdinalIgnoreCase))
                    if (tenantName.Equals("trainly", StringComparison.OrdinalIgnoreCase))
                        tenantName = "";
                }

                if (!string.IsNullOrEmpty(tenantName))
                {
                    if (!TenantConfigCollection.ContainsKey(tenantName))
                    {
                        lock (appSettingsLock)
                        {
                            if (!TenantConfigCollection.ContainsKey(tenantName))
                            {
                                // Carregar configurações específicas do tenant a partir do appsettings
                                var tenantPrefix = $"{tenantName}:";
                                var tenantSettings = ConfigurationManager.AppSettings
                                    .AllKeys
                                    .Where(k => k.StartsWith(tenantPrefix, StringComparison.OrdinalIgnoreCase))
                                    .ToDictionary(
                                        k => k.Substring(tenantPrefix.Length),
                                        k => ConfigurationManager.AppSettings[k]
                                    );

                                var all = new NameValueCollection(ConfigCollection);

                                foreach (var kvp in tenantSettings)
                                {
                                    all[kvp.Key] = kvp.Value;
                                }

                                // Aplicar chaves de debug para o tenant
                                foreach (var debugKey in all.AllKeys.Where(a => a.StartsWith("Debug:")))
                                {
                                    var originalKey = debugKey.Replace("Debug:", "");

                                    if (all.AllKeys.Contains(originalKey))
                                    {
                                        all[originalKey] = all[debugKey];
                                    }
                                    else
                                    {
                                        all.Add(originalKey, all[debugKey]);
                                    }
                                }

                                TenantConfigCollection.Add(tenantName, all);
                            }
                        }
                    }

                    if (AsyncLocalConfigCollection.Value != null)
                    {
                        var all = new NameValueCollection(TenantConfigCollection[tenantName]);

                        foreach (string key in AsyncLocalConfigCollection.Value)
                        {
                            all[key] = AsyncLocalConfigCollection.Value[key];
                        }

                        return all;
                    }

                    return TenantConfigCollection[tenantName];
                }

                if (AsyncLocalConfigCollection.Value != null)
                {
                    var all = new NameValueCollection(ConfigCollection);

                    foreach (string key in AsyncLocalConfigCollection.Value)
                    {
                        all[key] = AsyncLocalConfigCollection.Value[key];
                    }

                    return all;
                }

                return ConfigCollection;
            }
        }

    }
}


public class EngineInitializationParameters
{
    public Assembly ExecutingAssembly { get; set; }
    public ISchemaNameProvider NameProvider { get; set; }
    public IList<IStartupTask> StartupTasks { get; set; } = new List<IStartupTask>();
    public IList<IDependencySetup> DependencyRegistrators { get; set; } = new List<IDependencySetup>();
    public ConnectionStringBaseVM ConnectionStringBase { get; set; }
    public ContainerBuilder ContainerBuilder { get; set; }

    public EngineInitializationParameters()
    {
    }

    public EngineInitializationParameters(Assembly executingAssembly)
    {
        ExecutingAssembly = executingAssembly;
    }
}