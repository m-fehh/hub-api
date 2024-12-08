using Hub.Domain.Entity;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;

namespace Hub.Application.Services
{
    /// <summary>
    /// Serviço para armazenar informações diversas no momento do login
    /// <see href="https://dev.azure.com/evuptec/EVUP/_workitems/edit/17365/">Link do PBI</see>
    /// </summary>
    public class PortalUserFingerprintService : CrudServiceDefault<PortalUserFingerprint>
    {
        public PortalUserFingerprintService(IRepository<PortalUserFingerprint> repository) : base(repository)
        {
        }
    }
}
