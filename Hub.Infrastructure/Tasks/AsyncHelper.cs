using Hub.Infrastructure.Autofac;

namespace Hub.Infrastructure.Tasks
{
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
          TaskFactory(CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);

        /// <summary>
        /// Permite que um método asyncrono seja executado de forma sincrona. 
        /// ATENÇÃO: ele irá abrir uma nova sessão do AutoFac, isso implica que sessões com o banco (por exemplo) não podem ser reaproveitadas dentro do método (será aberta nova sessão)
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            var tenantName = Singleton<ISchemaNameProvider>.Instance.TenantName();

            Func<Task<TResult>> fn = async () =>
            {
                using (Engine.BeginLifetimeScope(tenantName))
                {
                    return await func();
                }
            };

            return AsyncHelper._myTaskFactory
              .StartNew<Task<TResult>>(fn)
              .Unwrap<TResult>()
              .GetAwaiter()
              .GetResult();
        }

        /// <summary>
        /// Permite que um método asyncrono seja executado de forma sincrona. 
        /// ATENÇÃO: ele irá abrir uma nova sessão do AutoFac, isso implica que sessões com o banco (por exemplo) não podem ser reaproveitadas dentro do método (será aberta nova sessão)
        /// </summary>
        /// <param name="func"></param>
        public static void RunSync(Func<Task> func)
        {
            var tenantName = Singleton<ISchemaNameProvider>.Instance.TenantName();

            Func<Task> fn = async () =>
            {
                using (Engine.BeginLifetimeScope(tenantName))
                {
                    await func();
                }
            };

            AsyncHelper._myTaskFactory
              .StartNew<Task>(fn)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }
}
