using Hub.Domain.Entity;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;

namespace Hub.Application.Services
{
    public class PortalUserFingerprintService : CrudServiceDefault<PortalUserFingerprint>
    {
        public PortalUserFingerprintService(IRepository<PortalUserFingerprint> repository) : base(repository) { }
    }
}
