using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Extensions;
using NHibernate.Cfg;
using NHibernate.Caches.CoreDistributedCache;
using NHibernate.Caches.CoreDistributedCache.Redis;
using NHibernate.Mapping.Attributes;
using System.Configuration;
using System.Reflection;
using System.Text;

namespace Hub.Infrastructure.Database.NhManagement
{
    public interface INhCustomConfiguration
    {
        void Configure(NhConfigurationData info, NhConfigurationMapeamento mapeamento);
    }

    /// <summary>
    /// Classe responsável por alimentar um objeto de configuração da ferramenta NHibernate.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    internal static class NhConfiguration
    {
        public static NHibernate.Cfg.Configuration Get(NhConfigurationData info, NhConfigurationMapeamento mapeamento)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if (mapeamento == null)
                throw new ArgumentNullException(nameof(mapeamento));

            var cfg = new NHibernate.Cfg.Configuration();

            cfg.SetProperty(NHibernate.Cfg.Environment.SessionFactoryName, info.TenantId);

            INhCustomConfiguration customConfiguration = null;

            if (Engine.TryResolve<INhCustomConfiguration>(out customConfiguration))
            {
                customConfiguration.Configure(info, mapeamento);
            }

            if (!string.IsNullOrEmpty(info.UseSecondLevelCache))
            {
                cfg.SetProperty(NHibernate.Cfg.Environment.UseSecondLevelCache, info.UseSecondLevelCache);
                cfg.SetProperty(NHibernate.Cfg.Environment.CacheProvider, info.CacheProvider);

                if (!string.IsNullOrEmpty(info.UseQueryCache))
                {
                    cfg.SetProperty(NHibernate.Cfg.Environment.UseQueryCache, info.UseQueryCache);
                }
            }
            if (Singleton<NhConfigurationTenant>.Instance != null && Singleton<NhConfigurationTenant>.Instance.ActiveNhProfiler)
            {
                cfg.SetProperty(NHibernate.Cfg.Environment.GenerateStatistics, "true");
            }

            var cs = info.ConnectionString;

            if (string.IsNullOrEmpty(cs)) cs = ConfigurationManager.ConnectionStrings[info.TenantId].ConnectionString;

            cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionString, cs);

            cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionProvider, info.ConnectionProvider);

            cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionDriver, info.ConnectionDriver);

            var isolation = ConfigurationManager.AppSettings["ConnectionIsolationLevel"];

            if (string.IsNullOrEmpty(isolation)) isolation = "ReadUncommitted";

            cfg.SetProperty(NHibernate.Cfg.Environment.Isolation, isolation);

            var dialect = info.Dialect;

            if (dialect == "NHibernate.Dialect.MsSql2008Dialect") dialect = "Trainly.Infrastructure.Data.Entity.FMKSQLDIalect";

            cfg.SetProperty(NHibernate.Cfg.Environment.Dialect, info.Dialect);

            cfg.SetProperty(NHibernate.Cfg.Environment.UseSqlComments, "true");

            cfg.SetProperty(NHibernate.Cfg.Environment.QuerySubstitutions, info.QuerySubstitutions);

            if (!string.IsNullOrEmpty(info.CommandTimeout))
                cfg.SetProperty(NHibernate.Cfg.Environment.CommandTimeout, info.CommandTimeout);
            else
                cfg.SetProperty(NHibernate.Cfg.Environment.CommandTimeout, "120");

            if (!string.IsNullOrEmpty(info.BatchSize)) cfg.SetProperty(NHibernate.Cfg.Environment.BatchSize, info.BatchSize);

            if (Engine.AppSettings["PrintNhQueries"].TryParseBoolean(false))
            {
                cfg.SetProperty(NHibernate.Cfg.Environment.ShowSql, "true");
                cfg.SetProperty(NHibernate.Cfg.Environment.FormatSql, "true");

                HbmSerializer.Default.Validate = true;
            }

            if (!string.IsNullOrEmpty(info.TransactionStrategy))
            {
                cfg.SetProperty(NHibernate.Cfg.Environment.TransactionStrategy, info.TransactionStrategy);
            }

            cfg.SetProperty(NHibernate.Cfg.Environment.CurrentSessionContextClass, info.CurrentSessionContext);

            cfg.SetProperty(NHibernate.Cfg.Environment.LinqToHqlGeneratorsRegistry, typeof(ExtendedLinqtoHqlGeneratorsRegistry).AssemblyQualifiedName);

            if (info.SchemaDefault != null) cfg.SetProperty(NHibernate.Cfg.Environment.DefaultSchema, info.SchemaDefault);

            HbmSerializer.Default.WriteDateComment = false;

            var tenants = Singleton<NhConfigurationTenant>.Instance;

            foreach (NhAssembly item in mapeamento.Assemblies)
            {
                var assemblyPath = tenants.AppPath + (tenants.AppPath.EndsWith(@"\") ? "" : @"\") + item.Name;

                var stream = new MemoryStream();

                HbmSerializer.Default.Serialize(stream, Assembly.LoadFrom(assemblyPath));

                var buffer = new byte[stream.Length];

                stream.Seek(0, SeekOrigin.Begin);

                stream.Read(buffer, 0, buffer.Length);

                var fullXml = Encoding.Default.GetString(buffer);

                var xml = fullXml.Substring(fullXml.IndexOf("<hibernate-mapping"));

                cfg.AddXmlString(xml);
            }

            if (info.CacheProvider?.Contains("CoreDistributedCacheProvider") ?? false)
            {
                var connectionString = Engine.ConnectionString("redis");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentNullException(nameof(connectionString));
                }
                var environment = Engine.AppSettings["environment"];
                if (string.IsNullOrEmpty(environment))
                {
                    throw new ArgumentNullException(nameof(environment));
                }
                var redisFactory = new NHibernate.Caches.CoreDistributedCache.Redis.RedisFactory(new Dictionary<string, string>()
                {
                    { "configuration", connectionString },
                    { "instance-name", environment }
                });

                CoreDistributedCacheProvider.CacheFactory = redisFactory;
            }

            return cfg;
        }
    }
}
