//using Hub.Domain.Entity;
//using Hub.Infrastructure;
//using Hub.Infrastructure.Database.NhManagement;
//using Hub.Infrastructure.Database.Services;
//using Hub.Infrastructure.Extensions;
//using Hub.Infrastructure.Localization;
//using Hub.Infrastructure.Redis;
//using Hub.Infrastructure.Security;

//namespace Hub.Application.Services
//{
//    public class OrganizationalStructureService : CrudService<OrganizationalStructure>
//    {
//        private static long? currentOrgStructureIfNull;

//        public OrganizationalStructureService(IRepository<OrganizationalStructure> repository) : base(repository) { }

//        private void Validate(OrganizationalStructure entity)
//        {
//            if (Queryable.Any(Table, u => u.Description == entity.Description && u.Id != entity.Id))
//            {
//                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Description));
//            }

//            if (Queryable.Any(Table, u => u.Abbrev == entity.Abbrev && u.Id != entity.Id))
//            {
//                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Abbrev));
//            }

//            if (entity.IsLeaf && Queryable.Any(Table, u => u.Father.Id == entity.Id))
//            {
//                throw new BusinessException(Engine.Get("OrgStructCantBeLast"));
//            }


//            if (entity.IsDomain)
//            {
//                ValidateDomainTree(entity);
//            }

//            ValidateIsParent(entity, entity.Father);
//        }

//        private void ValidateDomainTree(OrganizationalStructure entity)
//        {
//            this.ValidateDomainAncestors(entity);
//            this.ValidateDomainDescendants(entity);
//        }

//        private void ValidateDomainAncestors(OrganizationalStructure entity)
//        {
//            if (entity.Father != null)
//            {
//                var father = GetById(entity.Father.Id);

//                if (father.IsDomain)
//                    throw new BusinessException(Engine.Get("OrgStructAlreadyHaveAncestorDomain"));
//                else
//                    ValidateDomainAncestors(father);
//            }
//        }

//        private void ValidateDomainDescendants(OrganizationalStructure entity)
//        {
//            if (entity.Childrens != null && entity.Childrens.Count > 0)
//            {
//                if (entity.Childrens.Any(c => c.IsDomain))
//                    throw new BusinessException(Engine.Get("OrgStructAlreadyHaveDescendantDomain"));
//                else
//                {
//                    foreach (OrganizationalStructure child in entity.Childrens)
//                    {
//                        ValidateDomainDescendants(child);
//                    }
//                }
//            }
//        }

//        private void ValidateIsParent(OrganizationalStructure father, OrganizationalStructure children)
//        {
//            if (father == null || children == null) return;

//            children = GetById(children.Id);

//            if (children.Father == null) return;

//            if (children.Father == father)
//            {
//                throw new BusinessException(Engine.Get("OrgStructCircularRef"));
//            }

//            ValidateIsParent(father, GetById(children.Father.Id));
//        }

//        private void SetRootProperty(OrganizationalStructure entity)
//        {
//            if (entity.Father == null)
//                entity.IsRoot = true;
//            else
//                entity.IsRoot = false;
//        }

//        private string GenerateTree(OrganizationalStructure entity)
//        {
//            string returnList = "(" + entity.Id.ToString() + ")";

//            if (entity.Father != null)
//            {
//                var parent = _repository.Table.FirstOrDefault(p => p.Id == entity.Father.Id);

//                returnList = GenerateTree(parent) + "," + returnList;
//            }

//            return returnList;
//        }

//        private void ValidateInsert(OrganizationalStructure entity)
//        {
//            Validate(entity);
//        }


//        //public override long Insert(OrganizationalStructure entity)
//        //{
//        //    ValidateInsert(entity);

//        //    SetRootProperty(entity);

//        //    entity.Tree = GenerateTree(entity);

//        //    using (var transaction = base._repository.BeginTransaction())
//        //    {
//        //        var ret = base._repository.Insert(entity);

//        //        if (transaction != null) base._repository.Commit();

//        //        using (var transaction2 = base._repository.BeginTransaction())
//        //        {
//        //            var currentUser = (PortalUser)Engine.Resolve<ISecurityProvider>().GetCurrent();

//        //            if (currentUser != null)
//        //            {
//        //                currentUser.OrganizationalStructures.Add(entity);

//        //                Engine.Resolve<IRedisService>().Set($"UserOrgList{currentUser.Id}", null);

//        //                Engine.Resolve<IRepository<PortalUser>>().Update(currentUser);
//        //            }

//        //            entity.Tree = GenerateTree(entity);

//        //            base._repository.Update(entity);

//        //            if (transaction2 != null) base._repository.Commit();
//        //        }

//        //        return ret;
//        //    }
//        //}


//        public override void Delete(long id)
//        {
//            using (var transaction = base._repository.BeginTransaction())
//            {
//                var entity = base.GetById(id);

//                base._repository.Delete(entity);

//                if (transaction != null) base._repository.Commit();
//            }
//        }
//    }
//}


using Hub.Domain.Entity;
using Hub.Domain.Interfaces;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Exceptions;
using Hub.Infrastructure.Redis.Cache;
using Hub.Infrastructure.Redis;
using Hub.Infrastructure.Security;
using Hub.Infrastructure;
using Hub.Shared.Enums.Infrastructure;
using Hub.Shared.Interfaces.MultiTenant;
using Hub.Shared.Interfaces;
using MediatR;
using Newtonsoft.Json;
using NHibernate.Mapping;
using Hub.Infrastructure.Localization;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure.Database.Models;
using Hub.Infrastructure.Mediators;

namespace Hub.Application.Services
{
    public class OrganizationalStructureService : CrudService<OrganizationalStructure>, IOrgStructBasedService
    {
        private static object locker = new object();
        private static long? currentOrgStructureIfNull;

        public OrganizationalStructureService(IRepository<OrganizationalStructure> repository) : base(repository)
        {

        }

        private void Validate(OrganizationalStructure entity)
        {
            if (Table.Any(u => u.Description == entity.Description && u.Id != entity.Id))
            {
                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Description));
            }

            if (Table.Any(u => u.Abbrev == entity.Abbrev && u.Id != entity.Id))
            {
                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Abbrev));
            }

            if (entity.IsLeaf && Table.Any(u => u.Father.Id == entity.Id))
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

        private void ValidateInsert(OrganizationalStructure entity)
        {
            Validate(entity);
        }

        public override long Insert(OrganizationalStructure entity)
        {
            ValidateInsert(entity);

            SetRootProperty(entity);

            entity.Tree = GenerateTree(entity);

            using (var transaction = base._repository.BeginTransaction())
            {
                var ret = base._repository.Insert(entity);

                if (transaction != null) base._repository.Commit();

                using (var transaction2 = base._repository.BeginTransaction())
                {
                    var currentUser = (PortalUser)Engine.Resolve<ISecurityProvider>().GetCurrent();

                    if (currentUser != null)
                    {
                        currentUser.OrganizationalStructures.Add(entity);

                        Engine.Resolve<IRedisService>().Set($"UserOrgList{currentUser.Id}", null);

                        Engine.Resolve<IRepository<PortalUser>>().Update(currentUser);
                    }

                    entity.Tree = GenerateTree(entity);

                    base._repository.Update(entity);

                    if (transaction2 != null) base._repository.Commit();
                }

                return ret;
            }
        }

        public override void Update(OrganizationalStructure entity)
        {
            Validate(entity);

            SetRootProperty(entity);

            entity.Tree = GenerateTree(entity);

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
                var entity = base.GetById(id);
                base._repository.Delete(entity);
                if (transaction != null) base._repository.Commit();
            }
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

        private void SetRootProperty(OrganizationalStructure entity)
        {
            if (entity.Father == null)
                entity.IsRoot = true;
            else
                entity.IsRoot = false;
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

        public bool IsDomainStructure(long structId)
        {
            Func<long, bool> fn = (s) =>
            {
                return GetById(s).IsDomain;
            };

            return Engine.Resolve<CacheManager>().CacheAction(() => fn(structId));
        }

        public string GetConfigByNameFromRoot(string name)
        {
            var service = Engine.Resolve<IHubCurrentOrganizationStructure>();
            var root = service.GetCurrentRoot();
            var configName = GetConfigByName(root, name);
            return !string.IsNullOrWhiteSpace(configName) && configName != "-" ? configName : "";
        }

        public string GetConfigByName(OrganizationalStructure org, string name)
        {
            return GetConfigByName(org != null ? (long?)org.Id : null, name);
        }

        public string GetConfigByName(long? orgId_, string name_)
        {
            Func<long?, string, string> fn = (orgId, name) =>
            {
                var ret = "";

                var change = false;

                if (orgId == null)
                {
                    return Engine.Resolve<IRepository<OrgStructConfigDefault>>().Table.Where(c => c.Name == name).Select(c => c.DefaultValue).FirstOrDefault();
                }

                var redisService = Engine.Resolve<IRedisService>();

                var key = $"OrganizationalStructureConfigs{orgId}";

                var redisOrganizationalStructureConfigsString = redisService.Get(key).ToString();

                Dictionary<string, string> organizationalStructureConfigs = null;

                if (!string.IsNullOrEmpty(redisOrganizationalStructureConfigsString))
                {
                    organizationalStructureConfigs = JsonConvert.DeserializeObject<Dictionary<string, string>>(redisOrganizationalStructureConfigsString);
                }

                if (organizationalStructureConfigs == null)
                {
                    organizationalStructureConfigs = Engine.Resolve<IRepository<OrganizationalStructureConfig>>()
                        .Table.Where(c => c.OrganizationalStructure.Id == orgId)
                        .Select(x => new
                        {
                            x.Config.Name,
                            x.Value
                        })
                        .Distinct()
                        .ToList()
                        .ToDictionary(x => x.Name,
                            x => x.Value);

                    change = true;
                }

                if (organizationalStructureConfigs.ContainsKey(name))
                {
                    ret = organizationalStructureConfigs[name];
                }
                else
                {
                    var defaultConfig = Engine.Resolve<IRepository<OrgStructConfigDefault>>().CacheableTable.Where(c => c.Name == name).Select(c => c.DefaultValue).FirstOrDefault();

                    if (defaultConfig == null)
                        organizationalStructureConfigs.Add(name, "");
                    else
                        organizationalStructureConfigs.Add(name, defaultConfig);

                    change = true;

                    ret = defaultConfig;
                }

                if (change)
                {
                    redisService.Set(key, JsonConvert.SerializeObject(organizationalStructureConfigs));
                }

                return ret;
            };

            return Engine.Resolve<CacheManager>().CacheAction(() => fn(orgId_, name_), key: "OrganizationalStructureConfigs");

        }

        public void SetConfig(OrganizationalStructure org, string name, string value)
        {
            var redisService = Engine.Resolve<IRedisService>();

            var OrganizationalStructureConfigsString = redisService.Get("OrganizationalStructureConfigs" + org.Id).ToString();

            Dictionary<string, string> OrganizationalStructureConfigs = null;

            if (!string.IsNullOrEmpty(OrganizationalStructureConfigsString))
            {
                OrganizationalStructureConfigs = JsonConvert.DeserializeObject<Dictionary<string, string>>(OrganizationalStructureConfigsString);
            }

            if (OrganizationalStructureConfigs == null)
            {
                OrganizationalStructureConfigs = Engine.Resolve<IRepository<OrganizationalStructureConfig>>().Table.Where(c => c.OrganizationalStructure == org).Select(x => new { x.Config.Name, x.Value }).Distinct().ToList().ToDictionary(x => x.Name, x => x.Value);
            }
            else
            {
                if (!OrganizationalStructureConfigs.ContainsKey(name))
                    OrganizationalStructureConfigs.Add(name, value);
                else
                    OrganizationalStructureConfigs[name] = value;
            }

            redisService.Set("OrganizationalStructureConfigs" + org.Id, JsonConvert.SerializeObject(OrganizationalStructureConfigs));
        }

        public string GetCurrentConfigByName(string name)
        {
            var currentLevel = Engine.Resolve<IHubCurrentOrganizationStructure>().Get();

            return GetConfigByName(currentLevel.ToLong(), name);
        }

        public long? GetCurrentOrgStructureIfNull()
        {
            return currentOrgStructureIfNull;
        }

        public void CurrentOrgStructureIfNull(long orgId)
        {
            currentOrgStructureIfNull = orgId;
        }

        public Establishment GetCurrentEstablishment(string currentStringLevel = null)
        {
            if (string.IsNullOrEmpty(currentStringLevel))
            {
                currentStringLevel = Engine.Resolve<IHubCurrentOrganizationStructure>().Get();
            }

            if (string.IsNullOrEmpty(currentStringLevel)) return null;

            var currentLevel = long.Parse(currentStringLevel);

            return Engine.Resolve<IRepository<Establishment>>().Table.Where(r => r.OrganizationalStructure.Id == currentLevel).FirstOrDefault();
        }

        //public TimeZoneInfo GetCurrentEstablishmentTimeZone(string currentStringLevel = null)
        //{
        //    Func<string, TimeZoneInfo> fn = (orgLevel) =>
        //    {
        //        var redisService = Engine.Resolve<IRedisService>();

        //        var cachedTimeZone = redisService.Get($"TimeZone{orgLevel}").ToString();

        //        if (!string.IsNullOrEmpty(cachedTimeZone))
        //        {
        //            return TimeZoneInfo.FindSystemTimeZoneById(cachedTimeZone);
        //        }

        //        var establishemnt = Engine.Resolve<OrganizationalStructureService>().GetCurrentEstablishment(orgLevel);

        //        if (establishemnt != null && !string.IsNullOrEmpty(establishemnt.Timezone))
        //        {
        //            redisService.Set($"TimeZone{orgLevel}", establishemnt.Timezone);

        //            return TimeZoneInfo.FindSystemTimeZoneById(establishemnt.Timezone);
        //        }

        //        return null;
        //    };

        //    if (string.IsNullOrEmpty(currentStringLevel))
        //    {
        //        var localcached = Engine.Resolve<PortalCache>().Get().CurrentTimezone;

        //        if (!string.IsNullOrEmpty(localcached))
        //        {
        //            if (localcached == "-") return null;

        //            return TimeZoneInfo.FindSystemTimeZoneById(localcached);
        //        }

        //        currentStringLevel = Engine.Resolve<IHubCurrentOrganizationStructure>().Get();
        //    }

        //    if (string.IsNullOrEmpty(currentStringLevel)) return null;

        //    return Engine.Resolve<CacheManager>().CacheAction(() => fn(currentStringLevel));
        //}


        #region IOrgStructBasedService Methods

        public void LinkOwnerOrgStruct(IEntityOrgStructOwned entity)
        {
            var currentLevel = Engine.Resolve<IHubCurrentOrganizationStructure>().Get();

            if (string.IsNullOrEmpty(currentLevel))
            {
                if (currentOrgStructureIfNull == null)
                {
                    throw new BusinessException(Engine.Get("SelectOrgStruct"));
                }

                currentLevel = currentOrgStructureIfNull.ToString();
            }

            var orgStruct = GetById(long.Parse(currentLevel));

            if (orgStruct == null || orgStruct.Inactive)
            {
                throw new BusinessException(Engine.Get("SelectOrgStruct"));
            }

            entity.OwnerOrgStruct = orgStruct;
        }

        public bool AllowChanges<TEntity>(TEntity entity, bool thowsException = true) where TEntity : IBaseEntity, IEntityOrgStructOwned
        {
            var repository = Engine.Resolve<IRepository<TEntity>>();

            var currentOrgStruct = repository.Table.Where(e => e.Id == entity.Id).Select(e => e.OwnerOrgStruct).FirstOrDefault();

            var currentuserOrgIds = Engine.Resolve<UserService>().GetCurrentUserOrgList();

            if (currentuserOrgIds != null)
            {
                if (currentOrgStruct != null && !currentuserOrgIds.Contains(currentOrgStruct.Id))
                {
                    if (thowsException)
                    {
#if DEBUG
                        return false;
#else
                        throw new BusinessException(Engine.Get("NotAllowedChangedBecauseOwnerOrgStruct"));
#endif
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            entity.OwnerOrgStruct = currentOrgStruct;

            return true;
        }

        public void ChangeOwnerOrgStruct(string objectType, long objectId, long newOwnerOrgStructId)
        {
            using (var transaction = base._repository.BeginTransaction())
            {
                var type = Type.GetType($"SCH.Core.Entity.{objectType}");

                var hasModifitionControls = type?.GetInterfaces().Contains(typeof(IModificationControl));

                var newOwnerOrgStruct = GetById(newOwnerOrgStructId);

                if (hasModifitionControls ?? false)
                {
                    _repository.CreateQuery($"update {objectType} set OwnerOrgStruct = :newOrg, LastUpdateUTC = :lasUtc where Id = :objectId")
                        .SetParameter("newOrg", newOwnerOrgStruct)
                        .SetParameter("objectId", objectId)
                        .SetParameter("lasUtc", DateTime.UtcNow)
                        .ExecuteUpdate();

                }
                else
                {
                    _repository.CreateQuery($"update {objectType} set OwnerOrgStruct = :newOrg where Id = :objectId")
                        .SetParameter("newOrg", newOwnerOrgStruct)
                        .SetParameter("objectId", objectId)
                        .ExecuteUpdate();

                }

                if (transaction != null) base._repository.Commit();

                if (type != null)
                {
                    var notificationType = typeof(IUpdateEntityNotification<>).MakeGenericType(type);

                    object notificationObject;

                    if (Engine.TryResolve(notificationType, out notificationObject))
                    {
                        var idProd = notificationType.GetProperty("Id");
                        var actionProd = notificationType.GetProperty("Action");

                        idProd.SetValue(notificationObject, objectId);
                        actionProd.SetValue(notificationObject, ELogAction.Update);

                        Engine.Resolve<IMediator>().Publish((INotification)notificationObject);
                    }
                }
            }
        }

        public Dictionary<long, Dictionary<string, string>> GetConfigsByNameList(List<long> structureList, List<string> configKeys)
        {
            var dicRawConfigs = new Dictionary<long, Dictionary<string, string>>();

            var redisService = Engine.Resolve<IRedisService>();

            foreach (var structure in structureList)
            {
                var shouldUpdateRedisCache = false;

                var OrganizationalStructureConfigsString = redisService.Get("OrganizationalStructureConfigs" + structure).ToString();

                Dictionary<string, string> OrganizationalStructureConfigs = null;

                if (!string.IsNullOrEmpty(OrganizationalStructureConfigsString))
                    OrganizationalStructureConfigs = JsonConvert.DeserializeObject<Dictionary<string, string>>(OrganizationalStructureConfigsString);

                if (OrganizationalStructureConfigs == null)
                {

                    OrganizationalStructureConfigs = Engine.Resolve<IRepository<OrganizationalStructureConfig>>().Table.Where(c => c.OrganizationalStructure.Id == structure)
                                                                .Select(x => new { x.Config.Name, x.Value }).Distinct().ToList().ToDictionary(x => x.Name, x => x.Value);

                    shouldUpdateRedisCache = true;
                }

                var dicRawStructureConfigs = new Dictionary<string, string>();

                foreach (var name in configKeys)
                {
                    if (!OrganizationalStructureConfigs.ContainsKey(name))
                    {
                        var defaultConfig = Engine.Resolve<IRepository<OrgStructConfigDefault>>().CacheableTable.Where(c => c.Name == name).Select(c => c.DefaultValue).FirstOrDefault();

                        dicRawStructureConfigs.Add(name, defaultConfig);
                        OrganizationalStructureConfigs.Add(name, defaultConfig);

                        shouldUpdateRedisCache = true;

                    }
                    else
                    {
                        dicRawStructureConfigs.Add(name, OrganizationalStructureConfigs[name]);
                    }
                }

                dicRawConfigs.Add(structure, dicRawStructureConfigs);

                if (shouldUpdateRedisCache)
                {
                    redisService.Set("OrganizationalStructureConfigs" + structure, JsonConvert.SerializeObject(OrganizationalStructureConfigs));
                }
            }

            return dicRawConfigs;
        }

        #endregion


        /// <summary>
        /// Verifica se a configuração para uso do apelido está ativada para o domínio da unidade passada
        /// </summary>
        /// <param name="structureId">unidade ou domínio em que se dejesa verificar a configuração</param>
        /// <returns><b>False</b> caso o parametro seja o dominio raiz, caso contrário, busca no banco de dados o valor da configuração da unidade.</returns>
        public bool ClientUseNickName(long structureId)
        {
            if (structureId == 0)
            {
                return false;
            }

            var isCurrentOrgStructRoot = _repository.CacheableTable.Where(f => f.Id == structureId).Select(s => s.IsRoot).FirstOrDefault();

            if (isCurrentOrgStructRoot)
            {
                return false;
            }

            var organizationStructure = Engine.Resolve<IHubCurrentOrganizationStructure>();

            var currentDomain = long.Parse(organizationStructure.GetCurrentDomain(structureId.ToString()));

            return bool.Parse(GetConfigByName(currentDomain, "NicknameVisible"));
        }

        /// <summary>
        /// Método responsável por atualizar todas as lojas 
        /// </summary>
        /// <param name="organizationalStructureIds">Lista de ids das lojas que receberão atualização</param>
        public void SetOrganizationalStructureReadyToUpdateOnCMS(long cityId)
        {
            var schema = "sch" + Engine.Resolve<ITenantManager>().GetInfo().Id;

            var script = @"WITH cte AS (
                            SELECT DISTINCT e.OrganizationalStructureId
                            FROM {0}.Establishment e 
                            INNER JOIN {0}.OrganizationalStructure os ON e.OrganizationalStructureId = os.Id
                            INNER JOIN {0}.AddressCity ac ON e.CityId = ac.Id
                            INNER JOIN {0}.AddressCityRegion acr ON acr.AddressCityId = ac.Id
                            WHERE e.CityId = {1}
                            )

                            UPDATE os SET LastUpdateUTC = GETUTCDATE()
                            FROM {0}.OrganizationalStructure os 
                            INNER JOIN cte c ON c.OrganizationalStructureId = os.Id "
            ;

            Engine.Resolve<IRepository<OrganizationalStructure>>().CreateSQLQuery(string.Format(script, schema, cityId)).ExecuteUpdate();
        }

        /// <summary>
        /// Método responsável por gerar a árvore de estabelecimentos 
        /// </summary>
        /// <param name="removeNotAccessOrg"></param>
        /// <returns></returns>
        public List<OrganizationalStructureTreeModel> GenerateTreeList(bool removeNotAccessOrg = true)
        {

            var allItens = (from os in _repository.Table
                            where os.Inactive == false && os.Father != null
                            select new OrganizationalStructureVM
                            {
                                Id = os.Id,
                                Description = os.Description,
                                Father_Id = os.Father.Id,
                                Inactive = os.Inactive
                            }).ToList();

            var rootList = new List<OrganizationalStructureTreeModel>();

            rootList = (from r in _repository.Table
                        where r.Father == null
                        select new OrganizationalStructureTreeModel { Id = r.Id, Description = r.Description }).ToList();


            GenerateSubTreeList(rootList, allItens.ToList());

            return rootList;
        }

        private void GenerateSubTreeList(List<OrganizationalStructureTreeModel> rootList, List<OrganizationalStructureVM> allItens)
        {
            foreach (var item in rootList)
            {
                item.Items =
                    allItens.Where(i =>
                        i.Father_Id != null &&
                        i.Father_Id == item.Id)
                    .Select(i => new OrganizationalStructureTreeModel()
                    {
                        Id = i.Id.Value,
                        Description = i.Description,
                        FatherId = i.Father_Id,
                        Inactive = i.Inactive
                    }).ToList();

                GenerateSubTreeList(item.Items, allItens);
            }
        }

        /// <summary>
        /// Método responsável por retornar a lista de unidades
        /// </summary>
        /// <returns></returns>
        public List<OrganizationalStructureVM> GenerateEstablishmentList()
        {
            var allItens = (from os in _repository.Table
            where os.Inactive == false && os.Father != null
                            select new OrganizationalStructureVM
                            {
                                Id = os.Id,
                                Description = os.Description,
                                Father_Id = os.Father.Id
                            }).ToList();

            var rootList = (from r in _repository.Table
                            where r.Father == null && r.Inactive == false
                            select new OrganizationalStructureVM { Id = r.Id, Description = r.Description }).ToList();

            allItens.AddRange(rootList);

            return allItens;
        }

        public void UpdateLastUpdateUTC(long organizationalStructureId)
        {
            if (organizationalStructureId == 0) return;

            var establishmentRepository = Engine.Resolve<IRepository<Establishment>>();

            var schema = "sch" + Engine.Resolve<ITenantManager>().GetInfo().Id;

            establishmentRepository.CreateSQLQuery($"UPDATE {schema}.OrganizationalStructure SET LastUpdateUTC = GETUTCDATE() WHERE Id = {organizationalStructureId}").ExecuteUpdate();

            var establishmentId = establishmentRepository.Table.Where(w => w.OrganizationalStructure.Id == organizationalStructureId).Select(s => s.Id).FirstOrDefault();

            // se for grupo economico ou rede, pode ser que não exista o correspondente na Establishment
            if (establishmentId != 0)
            {
                establishmentRepository.CreateSQLQuery($"UPDATE {schema}.Establishment SET LastUpdateUTC = GETUTCDATE() WHERE Id = {establishmentId}").ExecuteUpdate();
            }
        }
    }
}