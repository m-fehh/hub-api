using Hub.Domain.Entity;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Mapper;
using Hub.Infrastructure.Redis.Cache;
using Hub.Infrastructure.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hub.Infrastructure.Database.Models;
using AutoMapper;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure;
using Hub.Shared.Enums.Infrastructure;
using Hub.Shared.Interfaces.MultiTenant;
using NHibernate.Linq;
using NHibernate.Transform;
using Hub.Infrastructure.Exceptions;
using Hub.Infrastructure.Extensions;

namespace Hub.Application.Services
{
    public class OrganizationalStructureConfigDTO { public string Value { get; set; } }


    public class OrganizationalStructureConfigService : CrudService<OrganizationalStructureConfig>
    {
        private readonly CacheManager _cacheManager;
        private readonly IRedisService _redisService;
        private readonly ICrudService<AccessRule> _accessRuleService;
        private readonly IModelEntityMapper<OrganizationalStructureConfig, OrganizationalStructureConfigVM> _modelMapperConfig;
        private readonly OrganizationalStructureService _organizationalStructureService;
        private readonly IRepository<OrgStructConfigDefault> _configDefaultService;

        public OrganizationalStructureConfigService(
            IRepository<OrganizationalStructureConfig> repository,
            CacheManager cacheManager,
            IRedisService redisService,
            ICrudService<AccessRule> accessRuleService,
            IModelEntityMapper<OrganizationalStructureConfig, OrganizationalStructureConfigVM> modelMapperConfig,
            OrganizationalStructureService organizationalStructureService,
            IRepository<OrgStructConfigDefault> configDefaultService)
            : base(repository)
        {
            this._cacheManager = cacheManager;
            this._redisService = redisService;
            this._accessRuleService = accessRuleService;
            this._modelMapperConfig = modelMapperConfig;
            this._organizationalStructureService = organizationalStructureService;
            this._configDefaultService = configDefaultService;
        }

        private void Validate(OrganizationalStructureConfig entity)
        {
            var config = (OrgStructConfigDefault)_repository.Refresh(entity.Config);

            if (config.ConfigType == "Int32")
            {
                if (string.IsNullOrEmpty(entity.Value))
                {
                    throw new BusinessException(string.Format(Engine.Get("DefaultRequiredMessage"), Engine.Get(config.Name)));
                }

                long v;

                if (!Int64.TryParse(entity.Value, out v))
                {
                    throw new BusinessException(string.Format(Engine.Get("generic_invalid_message"), Engine.Get(config.Name)));
                }
            }

            if (config.ConfigType == "Double")
            {
                if (string.IsNullOrEmpty(entity.Value))
                {
                    throw new BusinessException(string.Format(Engine.Get("DefaultRequiredMessage"), Engine.Get(config.Name)));
                }

                Double v;

                if (!Double.TryParse(entity.Value, out v))
                {
                    throw new BusinessException(string.Format(Engine.Get("generic_invalid_message"), Engine.Get(config.Name)));
                }
            }
        }

        public override long Insert(OrganizationalStructureConfig entity)
        {
            Validate(entity);

            AdjustConfigValue(entity);

            using (var transaction = base._repository.BeginTransaction())
            {
                var ret = base._repository.Insert(entity);

                var config = (OrgStructConfigDefault)Engine.Resolve<IRepository<OrgStructConfigDefault>>().Refresh(entity.Config);

                ClearCache(entity.OrganizationalStructure.Id, config);

                Engine.Resolve<OrganizationalStructureService>().SetConfig(entity.OrganizationalStructure, config.Name, entity.Value);

                if (transaction != null) base._repository.Commit();

                return ret;
            }
        }

        public override void Update(OrganizationalStructureConfig entity)
        {
            AdjustConfigValue(entity);

            Validate(entity);

            var schema = "sch" + Engine.Resolve<ITenantManager>().GetInfo().Id;
            var oldConfigValue = Engine.Resolve<IRepository<Establishment>>().CreateSQLQuery(String.Format("Select Value from {0}.OrganizationalStructureConfig Where Id = {1}", schema, entity.Id))
                .SetResultTransformer(Transformers.AliasToBean(typeof(OrganizationalStructureConfigDTO))).List<OrganizationalStructureConfigDTO>().FirstOrDefault();

            using (var transaction = base._repository.BeginTransaction())
            {
                LogChange(oldConfigValue.Value, entity);

                base._repository.Update(entity);

                var config = (OrgStructConfigDefault)Engine.Resolve<IRepository<OrgStructConfigDefault>>().Refresh(entity.Config);

                ClearCache(entity.OrganizationalStructure.Id, config);

                Engine.Resolve<OrganizationalStructureService>().SetConfig(entity.OrganizationalStructure, config.Name, entity.Value);

                if (transaction != null) base._repository.Commit();
            }
        }

        public override void Delete(long id)
        {
            using (var transaction = base._repository.BeginTransaction())
            {
                var entity = GetById(id);

                var config = (OrgStructConfigDefault)Engine.Resolve<IRepository<OrgStructConfigDefault>>().Refresh(entity.Config);

                ClearCache(entity.OrganizationalStructure.Id, config);

                Engine.Resolve<OrganizationalStructureService>().SetConfig(entity.OrganizationalStructure, config.Name, config.DefaultValue);

                base._repository.Delete(id);

                if (transaction != null) base._repository.Commit();
            }
        }

        public void AdjustConfigValue(OrganizationalStructureConfig entity)
        {
            if (string.IsNullOrEmpty(entity.Value))
            {
                entity.Value = "-";

                if (entity.Config.ConfigType != null)
                {
                    if (entity.Config.ConfigType.Equals("Boolean") || entity.Config.ConfigType.Equals("Double") || entity.Config.ConfigType.Equals("Int32"))
                    {
                        entity.Value = "0";
                    }
                }
            }
        }

        /// <summary>
        /// MÉTODO RESPONSÁVEL POR FAZER A GRAVAÇÃO DO LOG DE CONFIGURAÇÃO ALTERADA 
        /// </summary>
        /// <param name="oldValue">VALOR DA CONFIGURAÇÃO ANTIGA  NA BASE DE DADOS</param>
        /// <param name="model">NOVA CONFIGURAÇÃO</param>
        public bool LogChange(string oldValue, OrganizationalStructureConfig entity)
        {
            var configChanged = string.Compare(oldValue, entity.Value, true) != 0;

            if (configChanged)
            {
                string value;

                if (string.Compare(entity.Value, "True", true) == 0 || string.Compare(entity.Value, "False", true) == 0)
                {
                    value = string.Compare(entity.Value, "True", true) == 0 ? Engine.Get("Yes") : Engine.Get("No");

                }
                else
                {
                    value = entity.Value;
                }

                var organizationalStructureDescription = entity.OrganizationalStructure.Description;
                var orgStructConfigDefault = entity.Config;

                //a princípio o objeto vem preenchido, mas caso uma rotina não mapeada esteja utilizando,
                //preencher a informação a partir do banco de dados
                if (organizationalStructureDescription == null)
                {
                    organizationalStructureDescription = Engine.Resolve<OrganizationalStructureService>().Table
                        .Where(w => w.Id == entity.OrganizationalStructure.Id)
                        .Select(s => s.Description)
                        .FirstOrDefault();
                }

                if (orgStructConfigDefault?.GroupName == null || orgStructConfigDefault?.Name == null)
                {
                    orgStructConfigDefault = Engine.Resolve<IRepository<OrgStructConfigDefault>>().Table
                        .Where(w => w.Id == entity.Config.Id)
                        .Select(s => new OrgStructConfigDefault() { GroupName = s.GroupName, Name = s.Name })
                        .FirstOrDefault();
                }

                var message = string.Format(Engine.Get("OrganizationalStructureConfigChangedMessage"),
                                            organizationalStructureDescription,
                                            Engine.Get(orgStructConfigDefault.GroupName),
                                            Engine.Get(orgStructConfigDefault.Name),
                                            value);

                Engine.Resolve<LogService>().LogMessage(Engine.Get("ConfigEstrutOrg"), entity.Id, ELogAction.Update, 0, "", "", message);
            }

            return configChanged;
        }

        public int GetExpireConsentTermConfig()
        {
            var org = Engine.Resolve<OrganizationalStructureService>().Table.Where(w => w.IsRoot == true).FirstOrDefault();

            var configValue = Engine.Resolve<OrganizationalStructureService>().GetConfigByName(org, "ConsentTermExpireTime");

            if (string.IsNullOrEmpty(configValue) || configValue == "-")
            {
                return 0;
            }
            else
            {
                int _months = 0;

                int.TryParse(configValue, out _months);

                if (_months < 0)
                {
                    return 0;
                }
                else
                {
                    return Convert.ToInt32(configValue);
                }
            }
        }

        private void ClearCache(long organizatinalStructureId, OrgStructConfigDefault config)
        {
            _cacheManager.InvalidateCacheAction("OrganizationalStructureConfigs");

            string establishmentCacheKey = $"EstablishmentCacheConfig_{config.Name}";
            var entity = Engine.Resolve<OrganizationalStructureService>().GetById(organizatinalStructureId);
            if (entity != null && !entity.IsRoot)
                establishmentCacheKey += $"_{entity.Id}";

            _cacheManager.InvalidateCacheAction(establishmentCacheKey);
        }

        public void Update(List<OrganizationalStructureConfigVM> models)
        {
            if (models.Count == 0)
            {
                return;
            }

            var existingIds = models.Where(m => m.Id != null && m.Id != 0).Select(m => m.Id).ToList();
            var entities = this.Table
                .Fetch(c => c.Config)
                .ThenFetch(c => c.OrgStructConfigDefaultDependency)
                .Where(c => existingIds.Contains(c.Id)).ToList();

            var organizationalStructure = _organizationalStructureService.GetById(models[0].OrganizationalStructure_Id.Value);

            var configIds = models.Select(m => m.Config_Id).ToList();
            var configs = _configDefaultService.Table
                .Where(c => configIds.Contains(c.Id)).ToList();

            using (var transaction = base._repository.BeginTransaction())
            {
                try
                {
                    foreach (var model in models)
                    {
                        var entity = entities.Where(e => e.Id == model.Id).FirstOrDefault();
                        InsertOrUpdate(entity, model, organizationalStructure, configs);
                    }

                    if (transaction != null) base._repository.Commit();
                }
                catch (Exception ex)
                {
                    if (transaction != null) base._repository.RollBack();

                    //gerar log de erro
                    log4net.LogManager.GetLogger("Sch.OrganizationalStructureConfigService.Update").Error("Error updating configs", ex);
                    throw;
                }

            }

        }

        public void InsertOrUpdate(OrganizationalStructureConfig entity, OrganizationalStructureConfigVM model, OrganizationalStructure organizationalStructure, List<OrgStructConfigDefault> configs)
        {
            bool insert = entity == null;

            if (insert)
            {
                entity = _modelMapperConfig.BuildEntity(model);
            }
            else
            {
                Singleton<IMapper>.Instance.Map(model, entity);
            }

            //atribuir objetos completos para gravar o log corretamente
            entity.Config = configs.Where(c => c.Id == model.Config_Id).FirstOrDefault();
            entity.OrganizationalStructure = organizationalStructure;

            //se houve alteração na config, então salvar ou atualizar
            if (LogChange(model.OldValue, entity))
            {
                AdjustConfigValue(entity);

                if (insert)
                {
                    base._repository.Insert(entity);
                }
                else
                {
                    base._repository.Update(entity);
                }

                ClearCache(entity.OrganizationalStructure.Id, entity.Config);

                var config = (OrgStructConfigDefault)Engine.Resolve<IRepository<OrgStructConfigDefault>>().Refresh(entity.Config);
                Engine.Resolve<OrganizationalStructureService>().SetConfig(entity.OrganizationalStructure, config.Name, entity.Value);
            }
        }
    }
}
