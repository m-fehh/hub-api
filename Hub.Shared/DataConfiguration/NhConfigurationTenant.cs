using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Shared.DataConfiguration
{
    /// <summary>
    /// Classe responsável por armazenar uma coleção de configurações do NHibernate. 
    /// A classe NhConfiguration que fica dentro de VOEIT.FW.Data irá utilizar a Interface InhNameProvider para pegar o nome do atual Tenant (cada aplicação deverá definir como esse nome será formado) e localizar na coleção passada. 
    /// </summary>
    public class NhConfigurationTenant : ConfigurationSection
    {
        /// <summary>
        /// Local da aplicação. Utilizado para que o NHibernate localize as dlls que serão serializadas e terão as entidades reconhecidas.
        /// </summary>
        public string AppPath { get; set; }

        /// <summary>
        /// Define se o NHProf será ativado
        /// </summary>
        [ConfigurationProperty("ActiveNhProfiler", IsRequired = false, DefaultValue = false)]
        public bool ActiveNhProfiler
        {
            get { return (bool)this["ActiveNhProfiler"]; }
            set { this["ActiveNhProfiler"] = value; }
        }

        /// <summary>
        /// Coleção de mapeamentos
        /// </summary>
        [ConfigurationProperty("Mapeamentos")]
        [ConfigurationCollection(typeof(NhConfigurationMapeamentoCollection), AddItemName = "MapeamentoNH")]
        public NhConfigurationMapeamentoCollection Mapeamentos
        {
            get { return this["Mapeamentos"] as NhConfigurationMapeamentoCollection; }
            set { this["Mapeamentos"] = value; }
        }
    }

    /// <summary>
    /// Coleção da classe NhConfigurationMapeamento
    /// </summary>
    public class NhConfigurationMapeamentoCollection : ConfigurationElementCollection, IEnumerable<NhConfigurationMapeamento>
    {
        public NhConfigurationMapeamento this[int index]
        {
            get
            {
                return base.BaseGet(index) as NhConfigurationMapeamento;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public NhConfigurationMapeamento this[object index]
        {
            get
            {
                return base.BaseGet(index) as NhConfigurationMapeamento;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NhConfigurationMapeamento();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NhConfigurationMapeamento)element).MapeamentoId;
        }

        public void Add(NhConfigurationMapeamento element)
        {
            LockItem = false;  // the workaround
            BaseAdd(element);
        }

        public new IEnumerator<NhConfigurationMapeamento> GetEnumerator()
        {
            foreach (var key in this.BaseGetAllKeys())
            {
                yield return (NhConfigurationMapeamento)BaseGet(key);
            }
        }
    }

    /// <summary>
    /// Classe POCO que carrega as informações necessárias para configurar o NHibernate.
    /// </summary>
    public class NhConfigurationMapeamento : ConfigurationElement
    {
        [ConfigurationProperty("MapeamentoId", IsRequired = true)]
        public string MapeamentoId
        {
            get { return this["MapeamentoId"] as string; }
            set { this["MapeamentoId"] = value; }
        }

        /// <summary>
        /// Coleção de configurações (coleçao da classe NhConfigurationData)
        /// </summary>
        [ConfigurationProperty("Tenants")]
        [ConfigurationCollection(typeof(NhConfigurationDataCollection), AddItemName = "ConfigurationTenant")]
        public NhConfigurationDataCollection ConfigurationTenants
        {
            get { return this["Tenants"] as NhConfigurationDataCollection; }
            set { this["Tenants"] = value; }
        }

        /// <summary>
        /// Lista de assemblies que serão serializados pelo NHibernate afim de descobrir entidades
        /// </summary>
        [ConfigurationProperty("Assemblies")]
        [ConfigurationCollection(typeof(NhAssemblyCollection), AddItemName = "Assembly")]
        public NhAssemblyCollection Assemblies
        {
            get { return this["Assemblies"] as NhAssemblyCollection; }
            set { this["Assemblies"] = value; }
        }
    }

    /// <summary>
    /// Coleção da classe NhConfigurationData
    /// </summary>
    public class NhConfigurationDataCollection : ConfigurationElementCollection, IEnumerable<NhConfigurationData>
    {
        public NhConfigurationData this[int index]
        {
            get
            {
                return base.BaseGet(index) as NhConfigurationData;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public NhConfigurationData this[object index]
        {
            get
            {
                return base.BaseGet(index) as NhConfigurationData;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NhConfigurationData();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NhConfigurationData)element).TenantId;
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        public void Add(NhConfigurationData element)
        {
            LockItem = false;  // the workaround
            BaseAdd(element);
        }

        public new IEnumerator<NhConfigurationData> GetEnumerator()
        {
            foreach (var key in this.BaseGetAllKeys())
            {
                yield return (NhConfigurationData)BaseGet(key);
            }
        }
    }

    /// <summary>
    /// Classe POCO que carrega as informações necessárias para configurar o NHibernate.
    /// </summary>
    public class NhConfigurationData : ConfigurationElement, ICloneable
    {
        [ConfigurationProperty("TenantId", IsRequired = true)]
        public string TenantId
        {
            get { return this["TenantId"] as string; }
            set { this["TenantId"] = value; }
        }

        [ConfigurationProperty("ConnectionString", IsRequired = true)]
        public string ConnectionString
        {
            get { return this["ConnectionString"] as string; }
            set { this["ConnectionString"] = value; }
        }

        [ConfigurationProperty("ConnectionProvider", IsRequired = true)]
        public string ConnectionProvider
        {
            get { return this["ConnectionProvider"] as string; }
            set { this["ConnectionProvider"] = value; }
        }

        [ConfigurationProperty("ConnectionDriver", IsRequired = true)]
        public string ConnectionDriver
        {
            get { return this["ConnectionDriver"] as string; }
            set { this["ConnectionDriver"] = value; }
        }

        [ConfigurationProperty("Dialect", IsRequired = true)]
        public string Dialect
        {
            get { return this["Dialect"] as string; }
            set { this["Dialect"] = value; }
        }

        [ConfigurationProperty("CurrentSessionContext", IsRequired = true)]
        public string CurrentSessionContext
        {
            get { return this["CurrentSessionContext"] as string; }
            set { this["CurrentSessionContext"] = value; }
        }

        [ConfigurationProperty("SchemaDefault")]
        public string SchemaDefault
        {
            get { return this["SchemaDefault"] as string; }
            set { this["SchemaDefault"] = value; }
        }

        [ConfigurationProperty("QuerySubstitutions")]
        public string QuerySubstitutions
        {
            get { return this["QuerySubstitutions"] as string; }
            set { this["QuerySubstitutions"] = value; }
        }

        [ConfigurationProperty("CommandTimeout")]
        public string CommandTimeout
        {
            get { return this["CommandTimeout"] as string; }
            set { this["CommandTimeout"] = value; }
        }

        [ConfigurationProperty("BatchSize")]
        public string BatchSize
        {
            get { return this["BatchSize"] as string; }
            set { this["BatchSize"] = value; }
        }

        [ConfigurationProperty("UseSecondLevelCache")]
        public string UseSecondLevelCache
        {
            get { return this["UseSecondLevelCache"] as string; }
            set { this["UseSecondLevelCache"] = value; }
        }

        [ConfigurationProperty("UseQueryCache")]
        public string UseQueryCache
        {
            get { return this["UseQueryCache"] as string; }
            set { this["UseQueryCache"] = value; }
        }

        [ConfigurationProperty("CacheProvider")]
        public string CacheProvider
        {
            get { return this["CacheProvider"] as string; }
            set { this["CacheProvider"] = value; }
        }

        [ConfigurationProperty("TransactionStrategy")]
        public string TransactionStrategy
        {
            get { return this["TransactionStrategy"] as string; }
            set { this["TransactionStrategy"] = value; }
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    /// <summary>
    /// Coleção da classe NhAssembly
    /// </summary>
    public class NhAssemblyCollection : ConfigurationElementCollection, IEnumerable<NhAssembly>
    {
        public NhAssembly this[int index]
        {
            get
            {
                return base.BaseGet(index) as NhAssembly;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NhAssembly();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NhAssembly)element).Name;
        }

        public void Add(NhAssembly element)
        {
            LockItem = false;  // the workaround
            BaseAdd(element);
        }

        public new IEnumerator<NhAssembly> GetEnumerator()
        {
            foreach (var key in this.BaseGetAllKeys())
            {
                yield return (NhAssembly)BaseGet(key);
            }
        }
    }

    /// <summary>
    /// Classe para armazenar o nome das dlls que serão serializadas pelo NHibernate
    /// </summary>
    public class NhAssembly : ConfigurationElement
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return this["Name"] as string; }
            set { this["Name"] = value; }
        }
    }
}
