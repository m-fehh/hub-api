using Hub.Domain.Entity;
using Hub.Domain.Interfaces;
using Hub.Infrastructure;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure.Localization;
using Hub.Infrastructure.Redis;
using Hub.Infrastructure.Security;
using Newtonsoft.Json;

namespace Hub.Application.Services
{
    public class ProfileGroupService : CrudService<ProfileGroup>
    {
        private readonly IOrgStructBasedService orgStructBasedService;
        private readonly ISecurityProvider securityProvider;

        public ProfileGroupService(IRepository<ProfileGroup> repository, IOrgStructBasedService orgStructBasedService, ISecurityProvider securityProvider) : base(repository)
        {
            this.orgStructBasedService = orgStructBasedService;
            this.securityProvider = securityProvider;
        }

        private List<string> SaveAppProfileRoles(ProfileGroup entity)
        {
            var redisService = Engine.Resolve<IRedisService>();
            var cacheKey = $"ProfileRoles{entity.Id}";
            var value = new List<string>();

            if (entity.Administrator)
                value.Add("ADMIN");
            else
                value.AddRange(Table.Where(p => p.Id == entity.Id).SelectMany(p => p.Rules).Select(r => r.KeyName).ToList());

            redisService.Set(cacheKey, JsonConvert.SerializeObject(value));

            return value;
        }

        private void DeleteAppProfileRoles(long id)
        {
            var cacheKey = $"ProfileRoles{id}";
            var redisService = Engine.Resolve<IRedisService>();
            redisService.Delete(cacheKey);
        }

        public void Validate(ProfileGroup entity)
        {
            if (Table.Any(u => u.Name == entity.Name && u.Id != entity.Id))
            {
                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Name));
            }

            if (entity.DaysToInactivate == null)
            {
                throw new BusinessException(entity.DefaultRequiredMessage(e => e.DaysToInactivate));
            }

            if (entity.DaysToInactivate <= 0)
            {
                throw new BusinessException(Engine.Get("DaysToInactivateInvalid"));
            }
        }

        public void ValidadeInsert(ProfileGroup entity)
        {
            Validate(entity);
        }

        public override long Insert(ProfileGroup entity)
        {
            ValidadeInsert(entity);

            orgStructBasedService.LinkOwnerOrgStruct(entity);

            using (var transaction = base._repository.BeginTransaction())
            {
                var ret = base._repository.Insert(entity);

                if (transaction != null) base._repository.Commit();

                base._repository.Refresh(entity);

                SaveAppProfileRoles(entity);

                return ret;
            }
        }

        public override void Update(ProfileGroup entity)
        {
            Validate(entity);

            orgStructBasedService.AllowChanges(entity);

            var user = (PortalUser)Engine.Resolve<ISecurityProvider>().GetCurrent();

            var currentProfile = user.Profile;

            if (currentProfile.Id != entity.Id)
            {
                var dbRules = _repository.Table.Where(r => r.Id == entity.Id).SelectMany(r => r.Rules).ToList();

                if (!currentProfile.Administrator)
                {
                    var notContained = dbRules.Except(currentProfile.Rules);

                    foreach (var rule in notContained)
                    {
                        entity.Rules.Add(rule);
                    }
                }
            }
        }
}
