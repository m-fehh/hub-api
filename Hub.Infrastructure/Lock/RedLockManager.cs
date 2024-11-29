using Hub.Infrastructure.Builders;
using Hub.Infrastructure.Lock.Interfaces;
using Hub.Infrastructure.Redis;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

namespace Hub.Infrastructure.Lock
{
    /// <summary>
    /// Sistema para bloqueio distribuído (web farm) que utiliza o redis como controlador
    /// </summary>
    public class RedLockManager : ILockManager, IHealthChecker
    {
        public CheckerContainer CheckerContainer
        {
            get
            {
                return new CheckerContainerBuilder(this)
                    .AddItem(new CheckerItem<string>(() => Engine.AppSettings["environment"], e => !string.IsNullOrEmpty(e)))
                    .Build();
            }
        }

        private static object locker = new object();

        private RedLockFactory factory;

        public void Init()
        {
            if (factory == null)
            {
                lock (locker)
                {
                    if (factory == null)
                    {
                        var connects = new List<RedLockMultiplexer>()
                        {
                            Engine.Resolve<IRedisService>().GetRedisConnection()
                        };

                        factory = RedLockFactory.Create(connects);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (factory != null) factory.Dispose();
        }

        public IDisposable Lock(string resource, double expiryTimeInSeconds = 30)
        {
            var tries = 0;

            Init();

            IRedLock redLock = null;

            while (redLock == null || !redLock.IsAcquired)
            {
                tries++;

                redLock = factory.CreateLock((Engine.AppSettings["environment-redis"] ?? Engine.AppSettings["environment"]) + resource,
                    TimeSpan.FromSeconds(expiryTimeInSeconds),
                    TimeSpan.FromSeconds(expiryTimeInSeconds / 3),
                    TimeSpan.FromSeconds(1));

                if (!redLock.IsAcquired && tries >= 10)
                {
                    throw new Exception("RedLock not acquired");
                }
                else if (!redLock.IsAcquired)
                {
                    Thread.Sleep(3000);
                }
            }

            return redLock;
        }

        public async Task<IAsyncDisposable> LockAsync(string resource, double expiryTimeInSeconds = 30)
        {
            var tries = 0;

            Init();

            IRedLock redLock = null;

            while (redLock == null || !redLock.IsAcquired)
            {
                tries++;

                redLock = await factory.CreateLockAsync((Engine.AppSettings["environment-redis"] ?? Engine.AppSettings["environment"]) + resource,
                    TimeSpan.FromSeconds(expiryTimeInSeconds),
                    TimeSpan.FromSeconds(expiryTimeInSeconds / 3),
                    TimeSpan.FromSeconds(1));

                if (!redLock.IsAcquired && tries >= 10)
                {
                    throw new Exception("RedLock not acquired");
                }
                else if (!redLock.IsAcquired)
                {
                    Thread.Sleep(3000);
                }
            }

            return redLock;
        }

    }
}
