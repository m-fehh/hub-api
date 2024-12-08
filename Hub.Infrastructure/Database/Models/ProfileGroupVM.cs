using Hub.Infrastructure.Extensions;
using Hub.Shared.Interfaces;
using Newtonsoft.Json;

namespace Hub.Infrastructure.Database.Models
{
    public class ProfileGroupVM : BaseEntity, IProfileGroup
    {
        public override Int64 Id { get; set; }

        public string Name { get; set; }

        [JsonConverter(typeof(ConcreteListTypeConverter<IAccessRule, AccessRuleVM>))]
        public ICollection<IAccessRule> Rules { get; set; }

        public bool Administrator { get; set; }

        public bool IsSpecial { get; set; }
    }
}
