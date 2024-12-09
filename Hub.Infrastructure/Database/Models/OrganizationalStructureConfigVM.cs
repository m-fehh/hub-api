using Hub.Infrastructure.Database.Services;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Hub.Infrastructure.Database.Models
{
    public class OrganizationalStructureConfigVM : ICrudModel
    {
        public long? Id { get; set; }

        [JsonIgnore]
        public string SerializedOldValue { get; set; }

        [Display(Name = "OrganizationalStructure")]
        public long? OrganizationalStructure_Id { get; set; }

        [Display(Name = "OrganizationalStructure")]
        public string OrganizationalStructure_Name { get; set; }

        [Required]
        [Display(Name = "Config")]
        public long? Config_Id { get; set; }

        [Display(Name = "Name")]
        public string Config_Name { get; set; }

        [Display(Name = "GroupName")]
        public string Config_GroupName { get; set; }

        public string Config_GroupName_1
        {
            get
            {
                if (string.IsNullOrEmpty(Config_GroupName)) return null;

                return Config_GroupName.Split('/')[0];
            }
        }

        public string Config_GroupName_2
        {
            get
            {
                if (string.IsNullOrEmpty(Config_GroupName)) return null;

                var path = Config_GroupName.Split('/');

                if (path.Length <= 1) return null;

                return path[1];
            }
        }

        public string Config_GroupName_3
        {
            get
            {
                if (string.IsNullOrEmpty(Config_GroupName)) return null;

                var path = Config_GroupName.Split('/');

                if (path.Length <= 2) return null;

                return path[2];
            }
        }

        [Display(Name = "ConfigType")]
        public string Config_ConfigType { get; set; }

        [StringLength(300)]
        [Display(Name = "Value")]
        public string Value { get; set; }

        [Display(Name = "SearchName")]
        public string Config_SearchName { get; set; }

        [Display(Name = "SearchExtraCondition")]
        public string Config_SearchExtraCondition { get; set; }

        [Display(Name = "Value")]
        public Boolean BValue { get; set; }

        public bool Config_ApplyToRoot { get; set; }

        public bool Config_ApplyToDomain { get; set; }

        public bool Config_ApplyToLeaf { get; set; }

        public string Config_Options { get; set; }

        public string Legend { get; set; }

        public List<OrganizationalStructureConfigOptionVM> Options
        {
            get
            {
                if (String.IsNullOrEmpty(Config_Options) == true) return null;

                var items = JsonConvert.DeserializeObject<List<OrganizationalStructureConfigOptionVM>>(Config_Options);

                return items;
            }
        }

        public long? OrgStructConfigDefaultDependencyId { get; set; }

        public DateTime? LastUpdateUTC { get; set; }

        [Display(Name = "MaxLength")]
        public int? ConfigMaxLength { get; set; }

        public string OldValue { get; set; }
    }

    public class OrganizationalStructureConfigOptionVM
    {
        public string key { get; set; }

        public string value { get; set; }
    }
}
