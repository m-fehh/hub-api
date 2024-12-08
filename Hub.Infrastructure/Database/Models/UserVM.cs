using Hub.Infrastructure.Extensions;
using Hub.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Infrastructure.Database.Models
{
    public class UserVM : BaseEntity, IUser
    {
        public override Int64 Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string IpAddress { get; set; }

        [JsonConverter(typeof(ConcreteTypeConverter<ProfileGroupVM>))]
        public IProfileGroup Profile { get; set; }

        public long? DefaultOrgStructureId { get; set; }
        public List<long> OrgStructures { get; set; }
    }
}
