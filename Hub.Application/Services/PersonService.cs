using Hub.Domain.Entity;
using Hub.Domain.Interfaces;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure;
using Hub.Shared.Interfaces.MultiTenant;
using Hub.Infrastructure.Localization;

namespace Hub.Application.Services
{
    public class PersonService : CrudService<Person>
    {
        public PersonService(IRepository<Person> repository)
            : base(repository)
        {
        }


        private void Validate(Person entity)
        {
            if (string.IsNullOrEmpty(entity.Name))
            {
                throw new BusinessException(entity.DefaultRequiredMessage(e => e.Name));
            }

            if (string.IsNullOrEmpty(entity.Document))
            {
                throw new BusinessException(entity.DefaultRequiredMessage(e => e.Document));
            }

            if (Table.Any(u => u.Document == entity.Document && u.Id != entity.Id))
            {
                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Document));
            }
        }

        private void ValidateInsert(Person entity)
        {
            Validate(entity);

        }

        public override long Insert(Person entity)
        {
            ValidateInsert(entity);

            if (entity.OwnerOrgStruct == null)
            {
                Engine.Resolve<IOrgStructBasedService>().LinkOwnerOrgStruct(entity);
            }

            using (var transaction = base._repository.BeginTransaction())
            {
                var ret = base._repository.Insert(entity);

                if (transaction != null) base._repository.Commit();

                return ret;
            }
        }

        public override void Update(Person entity)
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

        public Person SavePerson(string document, string name, IList<OrganizationalStructure> structures, OrganizationalStructure ownerOrgStruct = null)
        {
            if (string.IsNullOrEmpty(document)) return null;

            document = document.Replace(".", "").Replace("-", "").Replace("/", "");

            var person = Table.FirstOrDefault(p => p.Document == document);

            if (person != null)
            {
                var schema = "sch" + Engine.Resolve<ITenantManager>().GetInfo().Id;

                person.Name = name;

                Update(person);

                _repository.Flush();

                _repository.CreateSQLQuery($"exec {schema}.usp_SavePersonStructures :personId, :structures")
                    .SetParameter("personId", person.Id)
                    .SetParameter("structures", string.Join(",", structures.Select(s => s.Id).ToArray()))
                    .ExecuteUpdate();
            }
            else
            {
                person = new Person()
                {
                    Document = document,
                    Name = name,
                    OrganizationalStructures = structures,
                    OwnerOrgStruct = ownerOrgStruct
                };

                Insert(person);
            }

            return person;
        }
    }
}
