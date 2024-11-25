using System.Configuration;

namespace Hub.Infrastructure.Database
{
    /// <summary>
    /// Classe responsável por armazenar uma coleção de configurações multi-tenant.
    /// </summary>
    public class SqlConfigurationTenant : ConfigurationSection
    {
        public string AppPath { get; set; }

        [ConfigurationProperty("ActiveProfiler", IsRequired = false, DefaultValue = false)]
        public bool ActiveProfiler
        {
            get { return (bool)this["ActiveProfiler"]; }
            set { this["ActiveProfiler"] = value; }
        }

        [ConfigurationProperty("Mapeamentos")]
        [ConfigurationCollection(typeof(SqlConfigurationMapeamentoCollection), AddItemName = "MapeamentoSQL")]
        public SqlConfigurationMapeamentoCollection Mapeamentos
        {
            get { return this["Mapeamentos"] as SqlConfigurationMapeamentoCollection; }
            set { this["Mapeamentos"] = value; }
        }
    }

    public class SqlConfigurationMapeamentoCollection : ConfigurationElementCollection, IEnumerable<SqlConfigurationMapeamento>
    {
        public SqlConfigurationMapeamento this[int index]
        {
            get => base.BaseGet(index) as SqlConfigurationMapeamento;
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SqlConfigurationMapeamento();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SqlConfigurationMapeamento)element).MapeamentoId;
        }

        public new IEnumerator<SqlConfigurationMapeamento> GetEnumerator()
        {
            foreach (var key in BaseGetAllKeys())
            {
                yield return (SqlConfigurationMapeamento)BaseGet(key);
            }
        }

        // Método público para adicionar um item
        public void Add(SqlConfigurationMapeamento mapping)
        {
            BaseAdd(mapping);
        }
    }

    public class SqlConfigurationMapeamento : ConfigurationElement
    {
        [ConfigurationProperty("MapeamentoId", IsRequired = true)]
        public string MapeamentoId
        {
            get { return (string)this["MapeamentoId"]; }
            set { this["MapeamentoId"] = value; }
        }

        [ConfigurationProperty("Tenants")]
        [ConfigurationCollection(typeof(SqlConfigurationDataCollection), AddItemName = "ConfigurationTenant")]
        public SqlConfigurationDataCollection ConfigurationTenants
        {
            get { return this["Tenants"] as SqlConfigurationDataCollection; }
            set { this["Tenants"] = value; }
        }

        [ConfigurationProperty("Assemblies")]
        [ConfigurationCollection(typeof(SqlAssemblyCollection), AddItemName = "Assembly")]
        public SqlAssemblyCollection Assemblies
        {
            get { return this["Assemblies"] as SqlAssemblyCollection; }
            set { this["Assemblies"] = value; }
        }
    }

    public class SqlConfigurationDataCollection : ConfigurationElementCollection, IEnumerable<SqlConfigurationData>
    {
        public SqlConfigurationData this[int index]
        {
            get => base.BaseGet(index) as SqlConfigurationData;
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SqlConfigurationData();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SqlConfigurationData)element).TenantId;
        }

        public new IEnumerator<SqlConfigurationData> GetEnumerator()
        {
            foreach (var key in BaseGetAllKeys())
            {
                yield return (SqlConfigurationData)BaseGet(key);
            }
        }

        // Método público para adicionar um item
        public void Add(SqlConfigurationData data)
        {
            BaseAdd(data);
        }
    }

    public class SqlConfigurationData : ConfigurationElement
    {
        [ConfigurationProperty("TenantId", IsRequired = true)]
        public string TenantId
        {
            get { return (string)this["TenantId"]; }
            set { this["TenantId"] = value; }
        }

        [ConfigurationProperty("ConnectionString", IsRequired = true)]
        public string ConnectionString
        {
            get { return (string)this["ConnectionString"]; }
            set { this["ConnectionString"] = value; }
        }

        [ConfigurationProperty("SchemaDefault", IsRequired = true)]
        public string SchemaDefault
        {
            get { return (string)this["SchemaDefault"]; }
            set { this["SchemaDefault"] = value; }
        }
    }

    public class SqlAssemblyCollection : ConfigurationElementCollection, IEnumerable<SqlAssembly>
    {
        public SqlAssembly this[int index]
        {
            get => base.BaseGet(index) as SqlAssembly;
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SqlAssembly();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SqlAssembly)element).Name;
        }

        public new IEnumerator<SqlAssembly> GetEnumerator()
        {
            foreach (var key in BaseGetAllKeys())
            {
                yield return (SqlAssembly)BaseGet(key);
            }
        }

        // Método público para adicionar um item
        public void Add(SqlAssembly assembly)
        {
            BaseAdd(assembly);
        }
    }

    public class SqlAssembly : ConfigurationElement
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["Name"]; }
            set { this["Name"] = value; }
        }
    }
}
