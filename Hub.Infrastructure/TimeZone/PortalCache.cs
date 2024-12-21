namespace Hub.Infrastructure.TimeZone
{
    /// <summary>
    /// cacheamento local por request ou lifetimescope para evitar multiplas consultas ao redis 
    /// </summary>
    public class PortalCache
    {
        /// <summary>
        /// faz a desambiguação entre o contexto web (per request) ou lifetimescope
        /// </summary>
        /// <returns></returns>
        public PortalCacheData Get()
        {
            return Engine.Resolve<PortalCacheRequest>();
        }
    }

    /// <summary>
    /// Dados que são cacheados
    /// </summary>
    public class PortalCacheData : IDisposable
    {
        public PortalCacheData()
        {
            InternalIdMap = new Dictionary<string, long>();
        }

        public string CurrentTimezone { get; set; }

        public string CurrentDomain { get; set; }

        /// <summary>
        /// Usado para mapear Ids vindos do cliente com objetos persistidos no servidor (telas offline com banco local)
        /// </summary>
        public Dictionary<string, long> InternalIdMap { get; set; }

        public void Dispose()
        {
            InternalIdMap.Clear();
        }
    }

    /// <summary>
    /// classe para registro no IOC por Request
    /// </summary>
    public class PortalCacheRequest : PortalCacheData
    {

    }
}
