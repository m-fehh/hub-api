using Hub.Infrastructure.Database.Services;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Hub.Infrastructure.Database.Models
{
    public class OrganizationalStructureVM : ICrudModel
    {
        public long? Id { get; set; }
        public string SerializedOldValue { get; set; }

        public string Alias
        {
            get
            {
                if (Abbrev == null) return null;

                return Abbrev.ToUpper();
            }
        }

        [Required]
        [StringLength(10)]
        [Display(Name = "Abbrev")]
        public string Abbrev { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Logo")]
        public string Logo { get; set; }

        [Display(Name = "Inactive")]
        public bool Inactive { get; set; }

        [Display(Name = "AppearInMobileApp")]
        public bool AppearInMobileApp { get; set; }

        public bool IsRoot { get; set; }

        [Display(Name = "IsEstablishment")]
        public bool IsLeaf { get; set; }

        /// <summary>
        /// Identifica se a estrutura é um domínio raiz de um grupo de estabelecimentos.
        /// </summary>
        [Display(Name = "IsDomain")]
        public bool IsDomain { get; set; }

        [Display(Name = "Father")]
        public long? Father_Id { get; set; }

        [Display(Name = "Father")]
        public string Father_Description { get; set; }

        /// <summary>
        /// Propriedade utilizada para identificar se o usuário atual pode gerenciar esse modelo (não utilizado para o cadastro)
        /// </summary>
        public bool IsEnabled { get; set; }

        [Display(Name = "SystemStartDateForInvoice")]
        public DateTime? SystemStartDateForInvoice { get; set; }

        [Display(Name = "CNPJ")]
        public string CNPJ { get; set; }

        [Display(Name = "AddressLat")]
        public virtual double? AddressLat { get; set; }

        [Display(Name = "AddressLng")]
        public virtual double? AddressLng { get; set; }

        [Display(Name = "Channels")]
        public virtual List<string> Channels { get; set; }

        [Display(Name = "AppChannel")]
        public virtual List<string> AppChannel { get; set; }

        //public List<SchedulerCalendarTimeDayVM> SchedulerCalendarDays { get; set; }

        public string PostalCode { get; set; }

        //public OrganizationalStructureAddressVM Address { get; set; }

        public virtual List<string> Emails { get; set; }
        public virtual List<OrganizationalStructurePhoneVM> Phones { get; set; }

        public virtual List<OrganizationalStructureGympassPlanVM> GympassPlans { get; set; }

        public virtual string CountryPhoneCode { get; set; }

        public virtual List<EmployeesVM> Employees { get; set; }

        //[JsonProperty("regions")]
        //public virtual List<CMSRegionVM> Regions { get; set; }

        //public virtual ESchedulerResourceClassifier? EnableSchedulerResources { get; set; }

        public virtual List<OrganizationalStructureResourceVM> Resources { get; set; }

        public DateTime? LastUpdateUTC { get; set; }
    }

    public class OrganizationalStructureAddressVM
    {
        public string PostalCode { get; set; }
        public EAddressType? eType { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Neighborhood { get; set; }
        public string Number { get; set; }
        public string Complement { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateAbrev { get; set; }
        public string Country { get; set; }
    }

    public class OrganizationalStructurePhoneVM
    {
        public string Number { get; set; }
        public string Type { get; set; }
        public string Extension { get; set; }
        public string Extension2 { get; set; }
    }

    public class OrganizationalStructureGympassPlanVM
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class OrganizationalStructureResourceVM
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public bool Inactive { get; set; }
        public bool IsEquipment { get; set; }
        public bool IsScheduler { get; set; }
        public int Quantity { get; set; }
    }

    public class EmployeesVM
    {
        public long Id { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeePosition { get; set; }
        public string ImageId { get; set; }
    }

    public class OrganizationalStructureTreeModel
    {
        public OrganizationalStructureTreeModel()
        {
            Items = new List<OrganizationalStructureTreeModel>();
        }

        public long Id { get; set; }
        public string Description { get; set; }
        public long? FatherId { get; set; }
        public bool Inactive { get; set; }

        public List<OrganizationalStructureTreeModel> Items { get; set; }
    }
}
