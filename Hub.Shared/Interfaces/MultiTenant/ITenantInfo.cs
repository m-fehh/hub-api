namespace Hub.Shared.Interfaces.MultiTenant
{
    public interface ITenantManager
    {
        ITenantInfo GetInfo();
    }

    public interface ITenantInfo
    {
        long Id { get; set; }

        string Name { get; set; }

        string Subdomain { get; set; }

        string SchemaName { get; set; }

        bool IsActive { get; set; }

        string CultureName { get; set; }
    }

    /// <summary>
    /// Implementação concreta de ITenantInfo.
    /// </summary>
    public class TenantInfo : ITenantInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Subdomain { get; set; }

        public string SchemaName { get; set; } 

        public bool IsActive { get; set; }  

        public string CultureName { get; set; }
    }
}
