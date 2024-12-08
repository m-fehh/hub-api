using Hub.Domain.Entity;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;

namespace Hub.Application.Services
{
    public class PortalUserPassHistoryService : CrudService<PortalUserPassHistory>
    {
        public PortalUserPassHistoryService(IRepository<PortalUserPassHistory> repository)
            : base(repository)
        {
        }


        private void Validate(PortalUserPassHistory entity)
        {
        }

        private void ValidateInsert(PortalUserPassHistory entity)
        {
            Validate(entity);

        }

        public override long Insert(PortalUserPassHistory entity)
        {
            ValidateInsert(entity);

            using (var transaction = base._repository.BeginTransaction())
            {
                var ret = base._repository.Insert(entity);

                if (transaction != null) base._repository.Commit();

                return ret;
            }
        }

        public override void Update(PortalUserPassHistory entity)
        {
            Validate(entity);

            using (var transaction = base._repository.BeginTransaction())
            {
                base._repository.Update(entity);

                if (transaction != null) base._repository.Commit();
            }
        }

        public override void Delete(long id)
        {
            using (var transaction = base._repository.BeginTransaction())
            {
                base._repository.Delete(id);

                if (transaction != null) base._repository.Commit();
            }
        }
    }
}
