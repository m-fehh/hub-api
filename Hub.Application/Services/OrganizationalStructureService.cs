using Hub.Domain.Entity;
using Hub.Infrastructure;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure.Localization;
using Hub.Infrastructure.Redis;
using Hub.Infrastructure.Security;

namespace Hub.Application.Services
{
    public class OrganizationalStructureService : CrudService<OrganizationalStructure>
    {
        private static long? currentOrgStructureIfNull;

        public OrganizationalStructureService(IRepository<OrganizationalStructure> repository) : base(repository) { }

        private void Validate(OrganizationalStructure entity)
        {
            if (Queryable.Any(Table, u => u.Description == entity.Description && u.Id != entity.Id))
            {
                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Description));
            }

            if (Queryable.Any(Table, u => u.Abbrev == entity.Abbrev && u.Id != entity.Id))
            {
                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Abbrev));
            }

            if (entity.IsLeaf && Queryable.Any(Table, u => u.Father.Id == entity.Id))
            {
                throw new BusinessException(Engine.Get("OrgStructCantBeLast"));
            }


            if (entity.IsDomain)
            {
                ValidateDomainTree(entity);
            }

            ValidateIsParent(entity, entity.Father);
        }

        private void ValidateDomainTree(OrganizationalStructure entity)
        {
            this.ValidateDomainAncestors(entity);
            this.ValidateDomainDescendants(entity);
        }

        private void ValidateDomainAncestors(OrganizationalStructure entity)
        {
            if (entity.Father != null)
            {
                var father = GetById(entity.Father.Id);

                if (father.IsDomain)
                    throw new BusinessException(Engine.Get("OrgStructAlreadyHaveAncestorDomain"));
                else
                    ValidateDomainAncestors(father);
            }
        }

        private void ValidateDomainDescendants(OrganizationalStructure entity)
        {
            if (entity.Childrens != null && entity.Childrens.Count > 0)
            {
                if (entity.Childrens.Any(c => c.IsDomain))
                    throw new BusinessException(Engine.Get("OrgStructAlreadyHaveDescendantDomain"));
                else
                {
                    foreach (OrganizationalStructure child in entity.Childrens)
                    {
                        ValidateDomainDescendants(child);
                    }
                }
            }
        }

        private void ValidateIsParent(OrganizationalStructure father, OrganizationalStructure children)
        {
            if (father == null || children == null) return;

            children = GetById(children.Id);

            if (children.Father == null) return;

            if (children.Father == father)
            {
                throw new BusinessException(Engine.Get("OrgStructCircularRef"));
            }

            ValidateIsParent(father, GetById(children.Father.Id));
        }

        private void SetRootProperty(OrganizationalStructure entity)
        {
            if (entity.Father == null)
                entity.IsRoot = true;
            else
                entity.IsRoot = false;
        }

        private string GenerateTree(OrganizationalStructure entity)
        {
            string returnList = "(" + entity.Id.ToString() + ")";

            if (entity.Father != null)
            {
                var parent = _repository.Table.FirstOrDefault(p => p.Id == entity.Father.Id);

                returnList = GenerateTree(parent) + "," + returnList;
            }

            return returnList;
        }

        private void ValidateInsert(OrganizationalStructure entity)
        {
            Validate(entity);
        }


        //public override long Insert(OrganizationalStructure entity)
        //{
        //    ValidateInsert(entity);

        //    SetRootProperty(entity);

        //    entity.Tree = GenerateTree(entity);

        //    using (var transaction = base._repository.BeginTransaction())
        //    {
        //        var ret = base._repository.Insert(entity);

        //        if (transaction != null) base._repository.Commit();

        //        using (var transaction2 = base._repository.BeginTransaction())
        //        {
        //            var currentUser = (PortalUser)Engine.Resolve<ISecurityProvider>().GetCurrent();

        //            if (currentUser != null)
        //            {
        //                currentUser.OrganizationalStructures.Add(entity);

        //                Engine.Resolve<IRedisService>().Set($"UserOrgList{currentUser.Id}", null);

        //                Engine.Resolve<IRepository<PortalUser>>().Update(currentUser);
        //            }

        //            entity.Tree = GenerateTree(entity);

        //            base._repository.Update(entity);

        //            if (transaction2 != null) base._repository.Commit();
        //        }

        //        return ret;
        //    }
        //}


        public override void Delete(long id)
        {
            using (var transaction = base._repository.BeginTransaction())
            {
                var entity = base.GetById(id);

                base._repository.Delete(entity);

                if (transaction != null) base._repository.Commit();
            }
        }
    }
}
