using Hub.Infrastructure.Extensions;
using Hub.Shared.Interfaces;
using Newtonsoft.Json;

namespace Hub.Infrastructure.Database.Models
{
    public class AccessRuleVM : BaseEntity, IAccessRule
    {
        public override Int64 Id { get; set; }

        [JsonConverter(typeof(ConcreteTypeConverter<AccessRuleVM>))]
        public IAccessRule Parent { get; set; }
        public string Description { get; set; }
        public string KeyName { get; set; }
    }
}
