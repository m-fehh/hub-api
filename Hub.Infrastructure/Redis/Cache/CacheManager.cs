using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Builders;
using Hub.Infrastructure.Lock.Interfaces;
using Hub.Infrastructure.Security;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Hub.Infrastructure.Redis.Cache
{
    public class CacheManager : IHealthChecker
    {
        public const string ProfileAndDomainLevel = "ProfileAndDomainLevel";
        public const string ProfileAndStructureLevel = "ProfileAndStructureLevel";
        public const string UserAndStructureLevel = "UserAndStructureLevel";
        public const string StructureLevel = "Structure";
        public const string UserLevel = "User";
        public const string TenantLevel = "Tenant";
        public const string EnvironmentLevel = "Environment";

        public CacheManager(IRedisService redisService)
        {
            this.redisService = redisService;
        }
        public CheckerContainer CheckerContainer
        {
            get
            {
                return new CheckerContainerBuilder(this)
                .AddItem(new CheckerItem<string>(() => Engine.AppSettings["environment"], e => !string.IsNullOrEmpty(e)))
                    .Build();
            }
        }

        /// <summary>
        /// Sistema usado para fazer cacheamento local (na máquina)
        /// </summary>
        private static MemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        private readonly IRedisService redisService;

        /// <summary>
        /// Opções padrões para serialização de objeto
        /// </summary>
        private readonly static Func<JsonSerializerSettings> getSerializeSettings = () =>
        {
            return new JsonSerializerSettings
            {
                TypeNameAssemblyFormatHandling = Newtonsoft.Json.TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects,
            };
        };

        /// <summary>
        /// Retorna uma string para ser usada como prefixo de chaves de cache. 
        /// Através do parametro level monta uma cadeia de texto concatenando os elementos necessários para gerar a chave.
        /// Ex. ProfileAndStructureLevel -> Irá montar uma string com o ID de perfil do usuário logado + a unidade selecionada.
        /// </summary>
        /// <param name="level">Valores aceitos:
        /// ProfileAndDomainLevel, ProfileAndStructureLevel, UserAndStructureLevel, Structure, User, Tenant
        /// </param>
        /// <returns></returns>
        public string CachePrefix(string level)
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;

            var environment = Engine.AppSettings["environment-redis"] ?? Engine.AppSettings["environment"];
            if (string.IsNullOrEmpty(environment))
            {
                throw new ArgumentNullException(nameof(environment));
            }

            if (level.Equals(CacheManager.EnvironmentLevel))
            {
                return $"{environment}{culture}";
            }

            var prefix = $"{Singleton<SchemaNameProvider>.Instance.TenantName()}{environment}{culture}";

            //if (level.Equals(CacheManager.ProfileAndDomainLevel))
            //{
            //    var currentDomain = Engine.Resolve<ICurrentOrganizationStructure>().GetCurrentDomain();

            //    var profileId = Engine.Resolve<ISecurityProvider>().GetCurrentProfileId();

            //    return $"{prefix}Profile{profileId}Domain{currentDomain?.Id}";
            //}
            //else if (level.Equals(CacheManager.ProfileAndStructureLevel))
            //{
            //    var currentOrganizationStructure = Engine.Resolve<ICurrentOrganizationStructure>().GetCurrent();

            //    var profileId = Engine.Resolve<ISecurityProvider>().GetCurrentProfileId();

            //    return $"{prefix}Profile{profileId}Structure{currentOrganizationStructure?.Id}";
            //}
            //else if (level.Equals(CacheManager.UserAndStructureLevel))
            //{
            //    var currentOrganizationStructure = Engine.Resolve<ICurrentOrganizationStructure>().GetCurrent();

            //    var userId = Engine.Resolve<ISecurityProvider>().GetCurrentId();

            //    return $"{prefix}User{userId}Structure{currentOrganizationStructure?.Id}";
            //}
            //else if (level.Equals(CacheManager.StructureLevel))
            //{
            //    var currentOrganizationStructure = Engine.Resolve<ICurrentOrganizationStructure>().GetCurrent();

            //    return $"{prefix}Structure{currentOrganizationStructure?.Id}";
            //}
            
            if (level.Equals(CacheManager.UserLevel))
            {
                var userId = Engine.Resolve<ISecurityProvider>().GetCurrentId();

                return $"{prefix}User{userId}";
            }
            else if (level.Equals(CacheManager.TenantLevel))
            {
                return prefix;
            }

            return null;
        }

        private object GetMemberExpressionValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        /// <summary>
        /// Monta uma chave única para representar a chave do cache
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="level"></param>
        /// <param name="key"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerMemberLine"></param>
        /// <returns></returns>
        private string GetCacheKey(IReadOnlyCollection<Expression> arguments, string level, string key, string callerMemberName, int? callerMemberLine)
        {
            ICollection<object> parameters = new List<object>();

            foreach (Expression expression in arguments)
            {
                var value = GetMemberExpressionValue(expression);

                parameters.Add(value?.ToString());
            }

            StringBuilder builder = new StringBuilder();

            builder.Append("CacheManager:");

            if (key != null)
            {
                builder.Append(key);
                builder.Append(":");
            }
            else
            {
                builder.Append($"{callerMemberName ?? "Global"}_{callerMemberLine ?? 0}:");
            }

            builder.Append(CachePrefix(level));
            builder.Append(":");

            parameters.ToList().ForEach(x =>
            {
                builder.Append("_");
                builder.Append(x);
            });

            return builder.ToString();
        }

        /// <summary>
        /// Tenta obter o resultado do cache local
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="localCacheTimeSeconds"></param>
        /// <param name="options">Opções de deserialização</param>
        /// <returns></returns>
        private (bool, T) GetFromLocal<T>(string cacheKey, int localCacheTimeSeconds, Func<JsonSerializerSettings> options = null)
        {
            if (localCacheTimeSeconds > 0)
            {
                object localCached = null;

                try
                {
                    localCached = cache.Get(cacheKey);
                }
                catch (ObjectDisposedException)
                {
                    try
                    {
                        lock (cache)
                        {
                            cache = new MemoryCache(new MemoryCacheOptions());

                            localCached = cache.Get(cacheKey);
                        }
                    }
                    catch (Exception) { }
                }

                if (localCached != null)
                {
                    var resultCached = JsonConvert.DeserializeObject<T>((string)localCached, options != null ? options() : null);

                    return (true, resultCached);
                }
            }

            return (false, default(T));
        }

        /// <summary>
        /// Tenta obter o cache do redis
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="localCacheTimeSeconds"></param>
        /// <param name="redisCacheTimeSeconds"></param>
        /// <param name="options">Opções para deserialização</param>
        /// <returns></returns>
        private (bool, T) GetFromRedis<T>(string cacheKey, int localCacheTimeSeconds, int redisCacheTimeSeconds, Func<JsonSerializerSettings> options = null)
        {
            if (redisCacheTimeSeconds > 0)
            {
                var serialized = redisService.Get(cacheKey).ToString();

                if (!string.IsNullOrEmpty(serialized))
                {
                    if (localCacheTimeSeconds > 0)
                    {
                        try
                        {
                            cache.Set(cacheKey, serialized, TimeSpan.FromSeconds(localCacheTimeSeconds));
                        }
                        catch (ObjectDisposedException)
                        {
                            try
                            {
                                lock (cache)
                                {
                                    cache = new MemoryCache(new MemoryCacheOptions());

                                    cache.Set(cacheKey, serialized, TimeSpan.FromSeconds(localCacheTimeSeconds));
                                }
                            }
                            catch (Exception) { }
                        }
                    }

                    var resultCached = JsonConvert.DeserializeObject<T>(serialized, options != null ? options() : null);

                    return (true, resultCached);
                }
            }

            return (false, default(T));
        }

        /// <summary>
        /// Serializa e armazena o resultado nos mecanismos de cache (local e redis)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="data"></param>
        /// <param name="localCacheTimeSeconds"></param>
        /// <param name="redisCacheTimeSeconds"></param>
        /// <param name="options">Opções de serialização, caso não informado será utilizado o padrão <see cref="getSerializeSettings"/></param>
        private void CacheResult<T>(string cacheKey, T data, int localCacheTimeSeconds, int redisCacheTimeSeconds, Func<JsonSerializerSettings> options = null)
        {
            options = options ?? getSerializeSettings;

            string result = JsonConvert.SerializeObject(data, settings: options());

            if (localCacheTimeSeconds > 0)
            {
                cache.Set(cacheKey, result, TimeSpan.FromSeconds(localCacheTimeSeconds));
            }

            if (redisCacheTimeSeconds > 0)
            {
                redisService.Set(cacheKey, result, TimeSpan.FromSeconds(redisCacheTimeSeconds));
            }
        }

        /// <summary>
        /// Wrapper para cacheamento de métodos. Usa duas estruturas de cache: local e redis.
        /// </summary>
        /// <typeparam name="T">Classe de retorno e objeto a ser cacheado</typeparam>
        /// <param name="action">Método a ser executado caso não encontre o cache</param>
        /// <param name="level">Usado para montar o prefixo da chave de cache. Valores aceitos: ProfileAndDomainLevel, ProfileAndStructureLevel, UserAndStructureLevel, Structure, User, Tenant</param>
        /// <param name="localCacheTimeSeconds">Quantidade de segundos que o sistema vai cachear localmente (se igual a zero, não usará o cache de memória, vai direto para o redis)</param>
        /// <param name="redisCacheTimeSeconds">Quantidade de segundos que o sistema vai cachear no redis</param>
        /// <param name="key"></param>
        /// <param name="callerMemberName">Nome do método pai que chamou a rotina. Usado como parte da chave de cacheamento</param>
        /// <param name="callerMemberLine"></param>
        /// <param name="options">Opções para serialização / deserialização</param>
        /// <returns></returns>
        public T CacheAction<T>(
            Expression<Func<T>> action,
            string level = "Tenant",
            int localCacheTimeSeconds = 15,
            int redisCacheTimeSeconds = 60,
            string key = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int? callerMemberLine = null,
            Func<JsonSerializerSettings> options = null)
        {
            var argumentsProperty = action.Body.GetType().GetProperty("Arguments");

            var arguments = (IReadOnlyCollection<Expression>)argumentsProperty.GetValue(action.Body);

            string cacheKey = GetCacheKey(arguments, level, key, callerMemberName, callerMemberLine);

            var (fromLocal, localValue) = GetFromLocal<T>(cacheKey, localCacheTimeSeconds, options);

            if (fromLocal) return localValue;

            var (fromRedis, redisValue) = GetFromRedis<T>(cacheKey, localCacheTimeSeconds, redisCacheTimeSeconds, options);

            if (fromRedis) return redisValue;

            if (redisCacheTimeSeconds > 0)
            {
                using (Engine.Resolve<ILockManager>().Lock(cacheKey))
                {
                    (fromRedis, redisValue) = GetFromRedis<T>(cacheKey, localCacheTimeSeconds, redisCacheTimeSeconds, options);

                    if (fromRedis) return redisValue;

                    T retrieve = action.Compile().Invoke();

                    CacheResult(cacheKey, retrieve, localCacheTimeSeconds, redisCacheTimeSeconds, options);

                    return retrieve;
                }
            }
            else
            {
                T retrieve = action.Compile().Invoke();

                CacheResult(cacheKey, retrieve, localCacheTimeSeconds, redisCacheTimeSeconds, options);

                return retrieve;
            }
        }

        /// <summary>
        /// Wrapper para cacheamento de métodos. Usa duas estruturas de cache: local e redis.
        /// </summary>
        /// <typeparam name="T">Classe de retorno e objeto a ser cacheado</typeparam>
        /// <param name="action">Método a ser executado caso não encontre o cache</param>
        /// <param name="level">Usado para montar o prefixo da chave de cache. Valores aceitos: ProfileAndDomainLevel, ProfileAndStructureLevel, UserAndStructureLevel, Structure, User, Tenant</param>
        /// <param name="localCacheTimeSeconds">Quantidade de segundos que o sistema vai cachear localmente (se igual a zero, não usará o cache de memória, vai direto para o redis)</param>
        /// <param name="redisCacheTimeSeconds">Quantidade de segundos que o sistema vai cachear no redis</param>
        /// <param name="key"></param>
        /// <param name="callerMemberName">Nome do método pai que chamou a rotina. Usado como parte da chave de cacheamento</param>
        /// <param name="callerMemberLine"></param>
        /// <param name="options">Opções para serialização / deserialização</param>
        public T CacheAction<T>(
           Expression<Func<Task<T>>> action,
           string level = "Tenant",
           int localCacheTimeSeconds = 15,
           int redisCacheTimeSeconds = 60,
           string key = null,
           [CallerMemberName] string callerMemberName = null,
           [CallerLineNumber] int? callerMemberLine = null,
           Func<JsonSerializerSettings> options = null)
        {
            var argumentsProperty = action.Body.GetType().GetProperty("Arguments");

            var arguments = (IReadOnlyCollection<Expression>)argumentsProperty.GetValue(action.Body);

            string cacheKey = GetCacheKey(arguments, level, key, callerMemberName, callerMemberLine);

            var (fromLocal, localValue) = GetFromLocal<T>(cacheKey, localCacheTimeSeconds, options);

            if (fromLocal) return localValue;

            var (fromRedis, redisValue) = GetFromRedis<T>(cacheKey, localCacheTimeSeconds, redisCacheTimeSeconds, options);

            if (fromRedis) return redisValue;

            if (redisCacheTimeSeconds > 0)
            {
                using (Engine.Resolve<ILockManager>().Lock(cacheKey))
                {
                    (fromRedis, redisValue) = GetFromRedis<T>(cacheKey, localCacheTimeSeconds, redisCacheTimeSeconds, options);

                    if (fromRedis) return redisValue;

                    var task = action.Compile().Invoke();

                    task.Wait();

                    CacheResult(cacheKey, task.Result, localCacheTimeSeconds, redisCacheTimeSeconds, options);

                    return task.Result;
                }
            }
            else
            {
                var task = action.Compile().Invoke();

                task.Wait();

                CacheResult(cacheKey, task.Result, localCacheTimeSeconds, redisCacheTimeSeconds, options);

                return task.Result;
            }
        }

        /// <summary>
        /// Wrapper para cacheamento de métodos. Usa duas estruturas de cache: local e redis.
        /// </summary>
        /// <typeparam name="T">Classe de retorno e objeto a ser cacheado</typeparam>
        /// <param name="action">Método a ser executado caso não encontre o cache</param>
        /// <param name="level">Usado para montar o prefixo da chave de cache. Valores aceitos: ProfileAndDomainLevel, ProfileAndStructureLevel, UserAndStructureLevel, Structure, User, Tenant</param>
        /// <param name="localCacheTimeSeconds">Quantidade de segundos que o sistema vai cachear localmente (se igual a zero, não usará o cache de memória, vai direto para o redis)</param>
        /// <param name="redisCacheTimeSeconds">Quantidade de segundos que o sistema vai cachear no redis</param>
        /// <param name="key"></param>
        /// <param name="callerMemberName">Nome do método pai que chamou a rotina. Usado como parte da chave de cacheamento</param>
        /// <param name="callerMemberLine"></param>
        /// <param name="options">Opções para serialização / deserialização</param>
        public async Task<T> CacheActionAsync<T>(
           Expression<Func<Task<T>>> action,
           string level = "Tenant",
           int localCacheTimeSeconds = 15,
           int redisCacheTimeSeconds = 60,
           string key = null,
           [CallerMemberName] string callerMemberName = null,
           [CallerLineNumber] int? callerMemberLine = null,
           Func<JsonSerializerSettings> options = null)
        {
            var argumentsProperty = action.Body.GetType().GetProperty("Arguments");

            var arguments = (IReadOnlyCollection<Expression>)argumentsProperty.GetValue(action.Body);

            string cacheKey = GetCacheKey(arguments, level, key, callerMemberName, callerMemberLine);

            var (fromLocal, localValue) = GetFromLocal<T>(cacheKey, localCacheTimeSeconds, options);

            if (fromLocal) return localValue;

            var (fromRedis, redisValue) = GetFromRedis<T>(cacheKey, localCacheTimeSeconds, redisCacheTimeSeconds, options);

            if (fromRedis) return redisValue;

            if (redisCacheTimeSeconds > 0)
            {
                using (Engine.Resolve<ILockManager>().Lock(cacheKey))
                {
                    (fromRedis, redisValue) = GetFromRedis<T>(cacheKey, localCacheTimeSeconds, redisCacheTimeSeconds, options);

                    if (fromRedis) return redisValue;

                    var task = action.Compile().Invoke();

                    T receive = await task;

                    CacheResult(cacheKey, receive, localCacheTimeSeconds, redisCacheTimeSeconds, options);

                    return receive;
                }
            }
            else
            {
                var task = action.Compile().Invoke();

                T receive = await task;

                CacheResult(cacheKey, receive, localCacheTimeSeconds, redisCacheTimeSeconds, options);

                return receive;
            }
        }

        /// <summary>
        /// Remove o cache a partir de uma key prédefinida
        /// </summary>
        /// <param name="key">Key usada no método <see cref="CacheAction{T}"/></param>
        public void InvalidateCacheAction(string key)
        {
            var redisService = Engine.Resolve<IRedisService>();

            redisService.DeleteFromPattern($"CacheManager:*{key}:*");

            lock (cache)
            {
                cache.Dispose();

                cache = new MemoryCache(new MemoryCacheOptions());
            }
        }
    }
}
