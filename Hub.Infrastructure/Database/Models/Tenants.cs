﻿using Hub.Infrastructure.MultiTenant;

namespace Hub.Infrastructure.Database.Models
{
    public class Tenants : ITenantInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Subdomain { get; set; }

        public string CultureName { get; set; }
    }
}
