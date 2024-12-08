using Hub.Domain.Entity;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure.Localization;

namespace Hub.Application.Services
{
    public class UserService : CrudService<PortalUser>
    {
        public bool RedirectOnAuthenticate { get; set; }

        public UserService(IRepository<PortalUser> repository) : base(repository)
        {
            RedirectOnAuthenticate = true;
        }

        private void Validate(PortalUser entity)
        {
            if (Queryable.Any(Table, u => u.Login == entity.Login && u.Id != entity.Id))
            {
                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Login));
            }
        }

        private void ValidadeInsert(PortalUser entity)
        {
            Validate(entity);
        }
    }
}
