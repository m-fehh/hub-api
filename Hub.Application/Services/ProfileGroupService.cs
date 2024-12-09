using Hub.Domain.Entity;
using Hub.Domain.Interfaces;
using Hub.Infrastructure;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Exceptions;
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

        #region PRIVATE METHODS 

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

        #endregion

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

            using (var transaction = base._repository.BeginTransaction())
            {
                base._repository.Update(entity);

                //Engine.Resolve<ProfileGroupAccessRequestService>().UpdateDelegateProfile(entity);

                if (transaction != null) base._repository.Commit();

                base._repository.Refresh(entity);

                SaveAppProfileRoles(entity);

                var culture = Thread.CurrentThread.CurrentCulture.Name;

                var redisTagName = $"UserProfileMenu{entity.Id}{culture}";

                var redisService = Engine.Resolve<IRedisService>();

                redisService.Set(redisTagName, null);

                //foreach (var item in Engine.Resolve<IRepository<PortalMenu>>().Table.Where(m => m.Name != "main-portal").ToList())
                //{
                //    redisTagName = $"UserProfileMenu{item.Name}{entity.Id}{culture}";

                //    redisService.Set(redisTagName, null);
                //}
            }
        }

        public override void Delete(long id)
        {
            if (Engine.Resolve<IRepository<PortalUser>>().Table.Any(p => p.Profile.Id == id))
            {
                throw new BusinessException(Engine.Get("CantDeletePortalUserReference"));
            }

            using (var transaction = base._repository.BeginTransaction())
            {
                var entity = GetById(id);

                base._repository.Delete(id);

                if (transaction != null) base._repository.Commit();

                DeleteAppProfileRoles(id);
            }
        }

        public IEnumerable<string> GetAppProfileRoles(long id)
        {
            var cacheKey = $"ProfileRoles{id}";
            var redisService = Engine.Resolve<IRedisService>();

            var profileRoles = redisService.Get(cacheKey).ToString();

            if (!string.IsNullOrEmpty(profileRoles))
                return JsonConvert.DeserializeObject<IEnumerable<string>>(profileRoles);

            ProfileGroup profile = Table.Where(p => p.Id == id).FirstOrDefault();

            if (profile == null)
                return new List<string>();

            return SaveAppProfileRoles(profile);
        }
    }
}
