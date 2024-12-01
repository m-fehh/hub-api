namespace Hub.Shared.Models.VMs
{
    public class TenantVM
    {
        public string Name { get; set; }
        public string Subdomain { get; set; }
        public bool IsActive { get; set; }
        public string CultureName { get; set; }

        public TenantVM(string name, string subdomain, bool isActive, string cultureName)
        {
            Name = name;
            Subdomain = subdomain;
            IsActive = isActive;
            CultureName = cultureName;
        }
    }
}
