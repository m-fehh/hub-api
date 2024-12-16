using Hub.Domain.Entity;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Shared.Interfaces;


namespace Hub.Domain.Database.Services
{
    public class AccessRuleService : CrudService<AccessRule>
    {
        public AccessRuleService(IRepository<AccessRule> repository) : base(repository) { }

        public override long Insert(AccessRule entity)
        {
            using (var transaction = _repository.BeginTransaction())
            {
                var ret = _repository.Insert(entity);

                entity.Tree = GenerateTree(entity);

                _repository.Update(entity);

                if (transaction != null) _repository.Commit();

                return ret;
            }
        }

        public override void Update(AccessRule entity)
        {
            entity.Tree = GenerateTree(entity);

            using (var transaction = _repository.BeginTransaction())
            {
                _repository.Update(entity);

                if (transaction != null) _repository.Commit();
            }
        }

        public override void Delete(long id)
        {
            using (var transaction = _repository.BeginTransaction())
            {
                var entity = base.GetById(id);

                _repository.Delete(entity);

                if (transaction != null) _repository.Commit();
            }
        }

        public string GenerateTree(IAccessRule entity)
        {
            if (entity == null) return "";

            string returnList = "(" + entity.Id.ToString() + ")";

            if (entity.Parent != null)
            {
                var parent = GetById(entity.Parent.Id);

                returnList = GenerateTree(parent) + "," + returnList;
            }

            return returnList;
        }
    }
}
