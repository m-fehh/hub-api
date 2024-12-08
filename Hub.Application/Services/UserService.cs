//using Hub.Domain.Entity;
//using Hub.Domain.Interfaces;
//using Hub.Infrastructure;
//using Hub.Infrastructure.Database.NhManagement;
//using Hub.Infrastructure.Database.Services;
//using Hub.Infrastructure.Extensions;
//using Hub.Infrastructure.Localization;

//namespace Hub.Application.Services
//{
//    public class UserService : CrudService<PortalUser>
//    {
//        public bool RedirectOnAuthenticate { get; set; }

//        public UserService(IRepository<PortalUser> repository) : base(repository)
//        {
//            RedirectOnAuthenticate = true;
//        }

//        private void Validate(PortalUser entity)
//        {
//            if (Queryable.Any(Table, u => u.Login == entity.Login && u.Id != entity.Id))
//            {
//                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Login));
//            }
//        }

//        private void ValidadeInsert(PortalUser entity)
//        {
//            Validate(entity);
//        }

//        public override long Insert(PortalUser entity)
//        {
//            ValidadeInsert(entity);

//            Engine.Resolve<IOrgStructBasedService>().LinkOwnerOrgStruct(entity);

//            using (var transaction = base._repository.BeginTransaction())
//            {
//                if (string.IsNullOrEmpty(entity.Password)) entity.Password = "voe[it{!@#}t^mp-p@ss]";

//                var ret = base._repository.Insert(entity);

//                if (transaction != null) base._repository.Commit();

//                return ret;
//            }
//        }

//        public override void Update(PortalUser entity)
//        {
//            Validate(entity);

//            Engine.Resolve<IOrgStructBasedService>().AllowChanges(entity);

//            if (string.IsNullOrEmpty(entity.Password))
//            {
//                entity.Password = Table.Where(u => u.Id == entity.Id).Select(p => p.Password).First();
//            }

//            using (var transaction = base._repository.BeginTransaction())
//            {
//                base._repository.Update(entity);

//                if (transaction != null) base._repository.Commit();
//            }
//        }

//        public override void Delete(long id)
//        {
//            var entity = GetById(id);

//            Engine.Resolve<IOrgStructBasedService>().AllowChanges(entity);

//            using (var transaction = base._repository.BeginTransaction())
//            {
//                base._repository.Delete(id);

//                if (transaction != null) base._repository.Commit();
//            }
//        }
//    }
//}


using Hub.Domain.Entity;
using Hub.Domain.Interfaces;
using Hub.Infrastructure;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure.Extensions.Generate;
using Hub.Infrastructure.Localization;
using Hub.Infrastructure.Redis;
using Hub.Infrastructure.Security;
using Hub.Shared.Enums;
using System.Linq;

namespace Hub.Application.Services
{
    public class UserService : CrudService<PortalUser>
    {
        private readonly IHubCurrentOrganizationStructure currentOrganizationStructure;
        private readonly IRedisService redisService;

        public UserService(IRepository<PortalUser> repository, IHubCurrentOrganizationStructure currentOrganizationStructure, IRedisService redisService) : base(repository) 
        {
            this.currentOrganizationStructure = currentOrganizationStructure;
            this.redisService = redisService;
        }

        private void Validate(PortalUser entity)
        {
            var profileRepository = Engine.Resolve<IRepository<ProfileGroup>>();

            var isAdmin = profileRepository.Table.Where(p => p.Id == entity.Profile.Id).Select(p => p.Administrator).FirstOrDefault();

            if (!isAdmin)
            {
                if (string.IsNullOrEmpty(entity.CpfCnpj))
                    throw new BusinessException(entity.DefaultRequiredMessage(e => e.CpfCnpj));

                if (Table.Any(u => u.CpfCnpj == entity.CpfCnpj && u.Id != entity.Id))
                    throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.CpfCnpj));
            }

            //apenas um admin pode manipular outro
            if (entity.Id != 0 && _repository.Table.Where(u => u.Id == entity.Id).Select(u => u.Profile.Administrator).FirstOrDefault())
            {
                var currentUser = Engine.Resolve<ISecurityProvider>().GetCurrentId();

                if (currentUser != null)
                {
                    if (!_repository.Table.Where(u => u.Id == currentUser).Select(u => u.Profile.Administrator).FirstOrDefault())
                    {
                        throw new BusinessException(Engine.Get("OnlyAdminCanChangeAdminUser"));
                    }
                }
            }

            if (Table.Any(u => u.Login == entity.Login && u.Id != entity.Id))
            {
                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Login));
            }

            if (Table.Any(u => u.Email == entity.Email && u.Id != entity.Id))
            {
                throw new BusinessException(entity.DefaultAlreadyRegisteredMessage(e => e.Email));
            }

            if (entity.OrganizationalStructures == null || entity.OrganizationalStructures.Count == 0)
            {
                throw new BusinessException(Engine.Get("UserOrgStructRequired"));
            }

            //Realiza validações na troca de Senha
            if (entity.ChangingPass)
            {
                ValidatePasswordHistory(entity);
            }
        }

        private void ValidatePasswordHistory(PortalUser entity)
        {
            if (entity.Password.Length < 8)
            {
                throw new BusinessException(Engine.Get("ErrorPasswordLength"));
            }

            var lastPasswords = Engine.Resolve<PortalUserPassHistoryService>().Get(w => w.User.Id == entity.Id,
                                                                      s => new PortalUserPassHistory
                                                                      {
                                                                          Password = s.Password,
                                                                          CreationUTC = s.CreationUTC
                                                                      }).OrderByDescending(o => o.CreationUTC)
                                                                      .Take(3)
                                                                      .Select(s => s.Password)
                                                                      .ToList();
            var password = entity.Password.EncodeSHA1();

            if (lastPasswords.Contains(password))
            {
                throw new BusinessException(Engine.Get("PasswordAlreadyUsed"));
            }
        }

        private void ValidadeInsert(PortalUser entity)
        {
            Validate(entity);

            if (string.IsNullOrEmpty(entity.TempPassword))
            {
                var authProvider = (EHubAuthProvider)Enum.Parse(typeof(EHubAuthProvider), Engine.AppSettings["Hub-auth-provider"]);

                if (authProvider == EHubAuthProvider.Native)
                {
                    throw new BusinessException(entity.DefaultRequiredMessage(e => e.TempPassword));
                }
            }
        }

        private void InsertPasswordChangeRecord(PortalUser entity)
        {
            var passwordExpirationDays = Engine.Resolve<ProfileGroupService>().Get(w => w.Id == entity.Profile.Id, s => s.PasswordExpirationDays).FirstOrDefault();
            var passHistory = new PortalUserPassHistory
            {
                User = entity,
                CreationUTC = DateTime.UtcNow,
                Password = entity.Password
            };

            if (passwordExpirationDays != EPasswordExpirationDays.Off)
            {
                passHistory.ExpirationUTC = passHistory.CreationUTC.AddDays((double)passwordExpirationDays);
            }

            Engine.Resolve<PortalUserPassHistoryService>().Insert(passHistory);
        }

        public override long Insert(PortalUser entity)
        {
            if (entity.DefaultOrgStructure == null && entity.OrganizationalStructures != null)
            {
                entity.DefaultOrgStructure = entity.OrganizationalStructures.FirstOrDefault();
            }

            ValidadeInsert(entity);

            entity.OwnerOrgStruct = Engine.Resolve<IHubCurrentOrganizationStructure>().GetCurrentRoot();

            using (var transaction = base._repository.BeginTransaction())
            {
                entity.Person = Engine.Resolve<PersonService>().SavePerson(entity.CpfCnpj, entity.Name, entity.OrganizationalStructures.ToList(), entity.OwnerOrgStruct);

                if (string.IsNullOrEmpty(entity.Password)) entity.Password = "voe[it{!@#}t^mp-p@ss]";

                entity.Password = entity.Password.EncodeSHA1();
                entity.Keyword = Engine.Resolve<UserKeywordService>().GenerateKeyword(entity.Name);

                var ret = base._repository.Insert(entity);

                Engine.Resolve<ICrudService<PortalUserSetting>>().Insert(new PortalUserSetting()
                {
                    PortalUser = entity,
                    Name = "current-organizational-structure",
                    Value = entity.DefaultOrgStructure.Id.ToString()
                });

                if (transaction != null) base._repository.Commit();

                return ret;
            }
        }

        public override void Update(PortalUser entity)
        {
            if (string.IsNullOrEmpty(entity.Keyword))
            {
                throw new BusinessException(entity.DefaultRequiredMessage(e => e.Keyword));
            }
            var userKeywordService = Engine.Resolve<UserKeywordService>();
            if (!userKeywordService.IsKeywordValid(entity.Keyword))
            {
                throw new BusinessException(Engine.Get("UserKeywordInvalid"));
            }
            if (userKeywordService.IsKeywordInUse(entity.Id, entity.Keyword))
            {
                throw new BusinessException(Engine.Get("UserKeywordInUse"));
            }

            if (entity.OrganizationalStructures == null || entity.OrganizationalStructures.Count == 0)
            {
                throw new BusinessException(Engine.Get("UserOrgStructRequired"));
            }

            SetUserDefaultOrganizationalStructure(entity);

            entity.OwnerOrgStruct = _repository.Table.Where(e => e.Id == entity.Id).Select(e => e.OwnerOrgStruct).FirstOrDefault();

            if (entity.ChangingPass && !string.IsNullOrEmpty(entity.Password))
            {
                entity.Password = entity.Password.EncodeSHA1();
            }
            else if (string.IsNullOrEmpty(entity.Password))
            {
                entity.Password = Table.Where(u => u.Id == entity.Id).Select(p => p.Password).First();
            }

            using (var transaction = base._repository.BeginTransaction())
            {

                entity.Person = Engine.Resolve<PersonService>().SavePerson(entity.CpfCnpj, entity.Name, entity.OrganizationalStructures.ToList());

                base._repository.Update(entity);

                if (entity.ChangingPass)
                {
                    InsertPasswordChangeRecord(entity);
                }

                if (transaction != null) base._repository.Commit();
            }

            currentOrganizationStructure.UpdateUser(entity.Id);

            #region Desabilita perfil temporário do usuário

            var tempProfile = UpdateTempProfile(entity);

            #endregion
        }

        /// <summary>
        /// Finaliza a requisição de passagem de acessos para o usuário que teve o perfil alterado.
        /// </summary>
        /// <param name="user"></param>
        private ProfileGroup UpdateTempProfile(PortalUser user)
        {
            var tempProfile = Engine.Resolve<IRepository<ProfileGroup>>().Table.Where(w => w.Id == user.Profile.Id).Select(s => s.TemporaryProfile).FirstOrDefault();

            if (tempProfile) return null;

            var profileRequestService = Engine.Resolve<IRepository<ProfileGroupAccessRequest>>();

            var userTempProfileApplied = profileRequestService.Table
                .Where(w => w.PortalUserReceived.Id == user.Id && w.Status == EProfileGroupAccessRequestStatus.Actual)
                .Select(s => s).FirstOrDefault();

            if (userTempProfileApplied != null)
            {
                userTempProfileApplied.Status = EProfileGroupAccessRequestStatus.Revoked;
                userTempProfileApplied.Inactive = true;

                using (var transaction = base._repository.BeginTransaction())
                {
                    profileRequestService.Update(userTempProfileApplied);

                    if (transaction != null) base._repository.Commit();
                }

                var cacheKey = $"UpdatedUserAccess{user.Id}";
                redisService.Set(cacheKey, user.Id.ToString(), TimeSpan.FromDays(7));

                return userTempProfileApplied.TemporaryProfile;
            }
            else
            {
                return null;
            }
        }

        public void UpdatePassword(PortalUser entity)
        {
            entity.TempPassword = null;
            entity.Password = entity.Password.EncodeSHA1();

            using (var transaction = base._repository.BeginTransaction())
            {
                base._repository.Update(entity);

                InsertPasswordChangeRecord(entity);

                if (transaction != null) base._repository.Commit();
            }
        }

        public void SetUserDefaultOrganizationalStructure(PortalUser portalUser)
        {
            var defaultOrgStructureExists = Engine.Resolve<IRepository<PortalUser>>().Table.Where(w => w.Id == portalUser.Id)
                                                 .Any(w => portalUser.OrganizationalStructures.Contains(w.DefaultOrgStructure));

            if (!defaultOrgStructureExists)
            {
                portalUser.DefaultOrgStructure = portalUser.OrganizationalStructures.FirstOrDefault();
            }
        }

        public PortalUser ResetPassword(string document, string newPassword)
        {
            var user = base.Table.FirstOrDefault(u => u.CpfCnpj.Equals(document));

            if (user != null && !string.IsNullOrEmpty(newPassword))
            {
                user.TempPassword = newPassword;
                user.LastPasswordRecoverRequestDate = DateTime.Now;

                base._repository.Update(user);

                return user;
            }

            return null;
        }

        public override void Delete(long id)
        {
            using (var transaction = base._repository.BeginTransaction())
            {
                var entity = GetById(id);
                base._repository.Delete(id);

                if (transaction != null) base._repository.Commit();
            }
        }

        public PortalUser CreateTempPassword(string userName)
        {
            var user = base.Table.FirstOrDefault(u => u.Login == userName || u.Email == userName);

            if (user != null)
            {
                user.TempPassword = Password.Generate(6, 1);
                user.LastPasswordRecoverRequestDate = DateTime.Now;

                Update(user);

                return user;
            }

            return null;
        }
    }
}