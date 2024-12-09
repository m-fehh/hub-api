using Hub.Domain.Entity;
using Hub.Infrastructure;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Exceptions;
using Hub.Infrastructure.Redis;
using Hub.Infrastructure.Security;
using Hub.Shared.Interfaces.MultiTenant;
using NHibernate.Transform;

namespace Hub.Application.Services
{
    public class EstablishmentService : CrudService<Establishment>
    {
        public EstablishmentService(IRepository<Establishment> repository) : base(repository) { }

        private void Validate(Establishment entity)
        {
            //if (entity.Classifier != null && entity.Classifier.ClassifierType != Enums.EClassifierType.Establishment)
            //{
            //    throw new BusinessException(Engine.Get("WrongEstablishmentClassifierType"));
            //}

            //if (entity.Emails.Where(x => x.Type == Enums.EEmailType.Principal && !x.DeleteFromList).Count() > 1)
            //{
            //    throw new BusinessException(Engine.Get("EstablishmentTwoMainEmails"));
            //}

            //if (entity.Emails.Where(x => x.Type == Enums.EEmailType.Principal && !x.DeleteFromList).Count() <= 0 && entity.Emails.Where(x => x.Type == Enums.EEmailType.Secundario && !x.DeleteFromList).Count() > 0)
            //{
            //    throw new BusinessException(Engine.Get("EstablishmentNoMainEmail"));
            //}

        }

        private void ValidateInsert(Establishment entity)
        {
            Validate(entity);
        }

        public override long Insert(Establishment entity)
        {
            ValidateInsert(entity);

            var profile = Engine.Resolve<ISecurityProvider>().GetCurrentProfile();

            var isAdmin = profile?.Administrator ?? false;

            if (!isAdmin) entity.SystemStartDate = null;

            long ret = 0;

            //var schema = "sch" + Engine.Resolve<ITenantManager>().GetInfo().Id;

            using (var transaction = base._repository.BeginTransaction())
            {
                ret = base._repository.Insert(entity);


                //base.SaveList(entity, entity.Emails);
                UpdateOrganizationalStructure(entity.OrganizationalStructure.Id);

                if (transaction != null) base._repository.Commit();
            }

            return ret;
        }

        /// <summary>
        /// RODAR UPDATE ORGANIZATIONALSTRUCTURE PARA ATULIZAR O CAMPO LastUpdateUTC
        /// </summary>
        /// <param name="organizationalStructureId"></param>
        public void UpdateOrganizationalStructure(long organizationalStructureId)
        {
            _repository.CreateQuery("update OrganizationalStructure set LastUpdateUTC= :date where Id =:id")
                .SetParameter("date", DateTime.UtcNow)
                .SetParameter("id", organizationalStructureId)
                .ExecuteUpdate();
        }
    }
}
