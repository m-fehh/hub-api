using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Database.Interfaces;
using StackExchange.Redis;

namespace Hub.Infrastructure.Redis
{
    public interface IRedisService : IDisposable
    {
        object Get(string key, bool ignoreTenant = false);
        void Set(string key, string value, TimeSpan? expiry = null);
        void Delete(string key);
        void DeleteFromPattern(string pattern);
        ConnectionMultiplexer GetRedisConnection(string connectionName = "redis");
    }

    public class RedisService : IRedisService
    {
        private string environment;

        private Dictionary<string, Lazy<ConnectionMultiplexer>> connections = new Dictionary<string, Lazy<ConnectionMultiplexer>>();

        private static object locker = new object();

        private static bool isConnecting = false;

        public RedisService()
        {
            environment = Engine.AppSettings["environment-redis"] ?? Engine.AppSettings["environment"];
            if (string.IsNullOrEmpty(environment))
            {
                throw new ArgumentNullException(nameof(environment));
            }
            connections.Add("redis", LazyConnection("redis"));
        }

        /// <summary>
        /// Cria um objeto Lazy para obtenção da conexão do redis
        /// </summary>
        /// <returns></returns>
        Lazy<ConnectionMultiplexer> LazyConnection(string connectionName)
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                isConnecting = true;

                try
                {
                    var connectionString = Engine.ConnectionString(connectionName);
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new ArgumentNullException(nameof(connectionString));
                    }
                    return ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(Engine.ConnectionString(connectionName)));
                }
                finally
                {
                    isConnecting = false;
                }

            }, true);
        }

        public ConnectionMultiplexer GetRedisConnection(string connectionName = "redis")
        {
            if (!connections.ContainsKey(connectionName))
            {
                lock (locker)
                {
                    if (!connections.ContainsKey(connectionName))
                    {
                        connections.Add(connectionName, LazyConnection(connectionName));
                    }
                }
            }

            return connections[connectionName].Value;
        }

        public object Get(string key, bool ignoreTenant = false)
        {
            object value = null;

            RetryTimeouts(() =>
            {
                value = GetRedisConnection().GetDatabase().StringGet(
                    ignoreTenant == false ?
                    GetComplexKey(key) :
                    key);
            });

            return value;
        }

        public void Set(string key, string value, TimeSpan? expiry = null)
        {
            RetryTimeouts(() =>
            {
                GetRedisConnection().GetDatabase().StringSet(GetComplexKey(key), value, expiry);
            });
        }

        public void Delete(string key)
        {
            RetryTimeouts(() =>
            {
                GetRedisConnection().GetDatabase().KeyDelete(GetComplexKey(key));
            });
        }

        public void Dispose()
        {
            GetRedisConnection()?.Dispose();
        }

        public void DeleteFromPattern(string pattern)
        {
            var tenantName = Singleton<ISchemaNameProvider>.Instance.TenantName();

            var connection = GetRedisConnection();

            var database = connection.GetDatabase();

            if (pattern.StartsWith("*"))
            {
                pattern = pattern.Substring(1);
            }

            foreach (var key in connection.GetEndPoints().SelectMany(e => connection.GetServer(e).Keys(0, $"{environment}:{tenantName}:*{pattern}")).ToList())
            {
                database.KeyDelete(key);
            }
        }

        void RetryTimeouts(Action action)
        {
            var counter = 0;

            var sleepTime = 500;

            Exception te = null;

            while (counter < 3)
            {
                try
                {
                    action();

                    return;
                }
                catch (TimeoutException e)
                {
                    te = e;

                    counter++;

                    Thread.Sleep(sleepTime);

                    sleepTime += 500;
                }
                catch (RedisConnectionException ce)
                {
                    if (counter > 0 && !isConnecting)
                    {
                        try
                        {
                            lock (locker)
                            {
                                if (connections.ContainsKey("redis"))
                                {
                                    connections.Remove("redis");
                                }
                            }
                        }
                        catch (Exception) { }
                    }

                    te = ce;

                    counter++;

                    Thread.Sleep(sleepTime);

                    sleepTime += 500;
                }
            }

            throw te;
        }

        private RedisKey GetComplexKey(string key)
        {
            var tenantName = Singleton<ISchemaNameProvider>.Instance.TenantName();
            return $"{environment}:{tenantName}:{key}";
        }
    }
}
