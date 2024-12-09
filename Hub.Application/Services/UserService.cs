using Hub.Domain.Entity;
using Hub.Domain.Interfaces;
using Hub.Infrastructure;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Exceptions;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure.Extensions.Generate;
using Hub.Infrastructure.Localization;
using Hub.Infrastructure.Redis;
using Hub.Infrastructure.Security;
using Hub.Infrastructure.Web;
using Hub.Shared.DataConfiguration;
using Hub.Shared.Enums;
using Hub.Shared.Interfaces;
using Hub.Shared.Models;
using Hub.Shared.Models.VMs;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Hub.Application.Services
{
    public class UserService : CrudService<PortalUser>, ISecurityProviderTemp
    {
        private const string TOKEN_KEY_USERID = ClaimTypes.NameIdentifier;
        private const string TOKEN_KEY_USERPROFILEID = "profileId";
        private DateTime lastSessionFactoryResetDate = DateTime.MinValue;

        private readonly IHubCurrentOrganizationStructure currentOrganizationStructure;
        private readonly IRedisService redisService;
        private readonly IUserProfileControlAccessService profileControlAccessService;
        private readonly IAccessTokenProvider accessTokenProvider;

        public UserService(IRepository<PortalUser> repository, IHubCurrentOrganizationStructure currentOrganizationStructure, IRedisService redisService, IAccessTokenProvider accessTokenProvider) : base(repository) 
        {
            this.currentOrganizationStructure = currentOrganizationStructure;
            this.redisService = redisService;
            this.accessTokenProvider = accessTokenProvider;
        }

        #region PRIVATE METHODS 

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

        #endregion

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

        public CheckUserProfileVM CheckUserProfile(long userId, long profileId)
        {
            var user = Engine.Resolve<IRepository<PortalUser>>().Table.Where(w => w.Id == userId).FirstOrDefault();
            var isTempProfile = false;
            var expireDate = string.Empty;
            var message = string.Empty;

            if (user != null)
            {
                var userProfile = Engine.Resolve<IRepository<ProfileGroup>>()
                    .Table.Where(w => w.Id == user.Profile.Id).Select(s => new { Id = s.Id, isTemp = s.TemporaryProfile, Name = s.Name }).FirstOrDefault();

                expireDate = Engine.Resolve<IRepository<ProfileGroupAccessRequest>>()
                    .Table.Where(w => w.ProfileCodeTemporary.Equals(userProfile.Name)).Select(s => s.EndValidity.ToString("dd/MM/yyyy")).FirstOrDefault();

                isTempProfile = false;

                if (isTempProfile && userProfile.Id != profileId)
                {
                    isTempProfile = true;
                    message = string.Format(Engine.Get("TempProfileAccessWillBeLost"), expireDate);
                }
                else
                {
                    isTempProfile = false;
                    message = string.Empty;
                }
            }

            return new CheckUserProfileVM
            {
                IsTempProfile = isTempProfile,
                WarningMessage = message
            };
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

        #region Authentication area

        public bool AuthenticateTemp(string userName, string tempPassword)
        {
            var currentUser = base.Table.FirstOrDefault(u => u.Login == userName && u.TempPassword == tempPassword);

            if (currentUser != null)
            {
                if (currentUser.Inactive)
                {
                    throw new BusinessException(Engine.Get("usuario_inativo"));
                }

                return true;
            }

            return false;
        }

        public void Authenticate(AuthVM request)
        {
            try
            {
                var message = AsyncHelper.RunSync(async () =>
                {
                    return await InternalApi.Request(new HttpEntityName(EHttpServiceType.SvcUser, EBusActionType.Query, "login"), JsonConvert.SerializeObject(new
                    {
                        Username = request.Login,
                        Password = request.Password
                    }));
                });

                var token = message.Check<string>("token");

                var tokenResult = Engine.Resolve<IAccessTokenProvider>().ValidateToken(token);

                if (tokenResult.Status != AccessTokenStatus.Valid)
                {
                    throw new BusinessException(Engine.Get("usuario_invalido"));
                }

                var portalUserId = long.Parse(accessTokenProvider.RetriveTokenData(tokenResult, TOKEN_KEY_USERID).Value);

                using (var trans = _repository.BeginTransaction())
                {

                    if (request.FingerPrint != null)
                    {
                        var portalUserFingerprint = new PortalUserFingerprint()
                        {
                            PortalUser = new PortalUser() { Id = portalUserId },
                            OS = request.FingerPrint.OS,
                            BrowserName = request.FingerPrint.BrowserName,
                            BrowserInfo = request.FingerPrint.BrowserInfo,
                            IpAddress = request.FingerPrint.IpAddress
                        };
                        Engine.Resolve<PortalUserFingerprintService>().Insert(portalUserFingerprint);
                    }

                    if (trans != null) _repository.Commit();
                }

                if (request.Provider == EAuthProvider.Form)
                {
                    HttpContextHelper.Current.Response.Cookies.Append("Authentication", token);

                    var parameter = new ProfileControlAccessVM(portalUserId, token);

                    var allowMultipleAccess = bool.Parse(accessTokenProvider.RetriveTokenData(tokenResult, "allowMultipleAccess")?.Value ?? "True");

                    profileControlAccessService.Save(parameter, allowMultipleAccess);
                }
                else
                {
                    token = GenerateJWT(portalUserId);
                }

                request.Token = token;

            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                if (NhGlobalData.CloseCurrentFactory != null)
                {
                    lock (NhGlobalData.CloseCurrentFactory)
                    {
                        if (lastSessionFactoryResetDate <= DateTime.Now.AddMinutes(-5)) //controle de tempo para não ficar resetando o tempo todo
                        {
                            lastSessionFactoryResetDate = DateTime.Now;

                            //Erro não explicado que acontece no ambiente multi-tenant. A aplicação se perde com as connections strings e acaba não reconhecendo o objeto na base.
                            //Fechamos a fábrica de sessão, para então, o sistema gerar uma nova fábrica e tentar se recuperar.
                            NhGlobalData.CloseCurrentFactory();
                        }
                    }
                }

                throw ex;
            }

        }

        public bool Authenticate(AuthenticationVM authenticationVM)
        {
            var model = new AuthVM()
            {
                Login = authenticationVM.UserName,
                Password = authenticationVM.Password,
                RememberMe = authenticationVM.RememberMe,
                Provider = EAuthProvider.Form,
                FingerPrint = (FingerPrintVM)authenticationVM.FingerPrint
            };

            //Authenticate(model);

            return (model.Token != null);
        }

        public void Authenticate(string token)
        {
            var accessService = Engine.Resolve<IAccessTokenProvider>();

            var tokenResult = accessService.ValidateExternalToken(token, Engine.AppSettings["etrust-secret-key"]);

            PortalUser currentUser = null;

            if (tokenResult != null)
            {
                HttpContextHelper.Current.Response.Cookies.Append("jsession", token);

                accessService.ValidateTokenStatus(tokenResult);
                var jwtHandler = new JwtSecurityTokenHandler();
                var tokenRead = jwtHandler.ReadJwtToken(token);
                var tokenUser = tokenRead.Claims.Where(w => w.Type.Equals("user")).FirstOrDefault()?.Value;

                if (string.IsNullOrEmpty(tokenUser) == false)
                {
                    var userJwtTokenKeys = JsonConvert.DeserializeObject<UserJwtTokenKeys>(tokenUser);

                    var userIdClaim = userJwtTokenKeys.userId;
                    var userEmailClaim = userJwtTokenKeys.userEmail;
                    var userDocClaim = userJwtTokenKeys.userDoc;

                    var portalUserTable = Engine.Resolve<ICrudService<PortalUser>>().Table;

                    if (currentUser == null && string.IsNullOrWhiteSpace(userIdClaim) == false)
                    {
                        var userId = long.Parse(userIdClaim);

                        currentUser = portalUserTable.FirstOrDefault(f => f.Id == userId && f.Inactive == false);
                    }

                    if (currentUser == null && string.IsNullOrWhiteSpace(userEmailClaim) == false)
                    {
                        var userEmail = long.Parse(userEmailClaim);
                        currentUser = portalUserTable.FirstOrDefault(w => w.Email.Equals(userEmail) && w.Inactive == false);
                    }

                    if (currentUser == null && string.IsNullOrWhiteSpace(userDocClaim) == false)
                    {
                        currentUser = portalUserTable.FirstOrDefault(w => w.CpfCnpj.Equals(userDocClaim) && w.Inactive == false);
                    }
                }
            }

            if (currentUser == null)
            {
                CookieExtensions.CleanCookies();
                HttpContextHelper.Current.Response.Redirect("~/Login", false);
            }

            var authData = new AuthUserToken { UserId = currentUser.Id, UserProfileId = currentUser.Profile.Id };

            var newToken = GenerateJWT(authData);

            HttpContextHelper.Current.Response.Cookies.Append("Authentication", newToken);

            var parameter = new ProfileControlAccessVM(currentUser.Id, newToken);

            profileControlAccessService.Save(parameter, ((ProfileGroup)currentUser.Profile).AllowMultipleAccess);
        }

        public void AuthenticateByFormsAuthentication(string token)
        {
            if (string.IsNullOrWhiteSpace(token) == false)
            {
                var tokenResult = accessTokenProvider.ValidateToken(token);

                accessTokenProvider.ValidateTokenStatus(tokenResult);

                HttpContextHelper.Current.Response.Cookies.Append("Authentication", token);
            }
        }

        public bool Authorize(string role)
        {
            var authCookie = HttpContextHelper.Current.Request.Cookies["Authentication"];

            if (authCookie != null && string.IsNullOrEmpty(authCookie) == false)
            {
                var tokenResult = accessTokenProvider.ValidateToken(authCookie);

                accessTokenProvider.ValidateTokenStatus(tokenResult);

                var claim = accessTokenProvider.RetriveTokenData(tokenResult, TOKEN_KEY_USERPROFILEID);
                var portalUserId = accessTokenProvider.RetriveTokenData(tokenResult, TOKEN_KEY_USERID);

                if (!string.IsNullOrWhiteSpace(portalUserId?.Value))
                {
                    var cacheKey = $"UpdatedUserAccess{portalUserId.Value}";
                    var redisService = Engine.Resolve<IRedisService>();

                    var userToRevoke = redisService.Get(cacheKey).ToString();

                    if (!string.IsNullOrWhiteSpace(userToRevoke))
                    {
                        CookieExtensions.CleanCookies();
                        redisService.Delete(cacheKey);

                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(claim?.Value))
                {
                    var profileId = long.Parse(claim.Value);

                    var profileGroupService = (ProfileGroupService)Engine.Resolve<ICrudService<ProfileGroup>>();

                    return profileGroupService.GetAppProfileRoles(profileId).Any(r => r == role || r == "ADMIN");
                }
            }

            if (bool.Parse(Engine.AppSettings["EnableAnonymousLogin"]))
            {
                var profileGroupService = (ProfileGroupService)Engine.Resolve<ICrudService<ProfileGroup>>();

                return profileGroupService.GetAppProfileRoles(1).Any(r => r == role || r == "ADMIN");
            }

            return false;
        }

        #endregion

        public string GenerateJWT(AuthUserToken tokenData)
        {
            var tokenExpirationTimeInMinutes = double.Parse(Engine.AppSettings["Hub-auth-token-expiration-time"]);

            return accessTokenProvider.GenerateToken(new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, tokenData.UserId.ToString()),
                        new Claim(TOKEN_KEY_USERID, tokenData.UserId.ToString()),
                        new Claim(TOKEN_KEY_USERPROFILEID, tokenData.UserProfileId.ToString())
                    }, expiryInMinutes: tokenExpirationTimeInMinutes);
        }

        public string GenerateJWT(long? userId = null)
        {
            if (userId == null) userId = GetCurrentId();

            var organizationalStructureService = Engine.Resolve<OrganizationalStructureService>();
            var rootOrg = organizationalStructureService.Table.Where(w => w.IsRoot == true).Select(s => s.Id).FirstOrDefault();
            var elosClientId = organizationalStructureService.GetConfigByName(new OrganizationalStructure() { Id = rootOrg }, "ElosClientId");

            return accessTokenProvider.GenerateToken(new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim("idrede", elosClientId)
                    }, expiryInMinutes: 1);
        }

        public void SetCurrentUser(string token)
        {
            //var user = ValidateToken(token);

            //CurrentUser.Value = user;
        }

        public void SetCurrentUser(IUser user)
        {
            //CurrentUser.Value = user;
        }

        public List<string> GetAuthorizedRoles(List<string> roles)
        {

            var authCookie = HttpContextHelper.Current.Request.Cookies["Authentication"];

            var profile = new List<string>();

            if (!string.IsNullOrEmpty(authCookie))
            {
                var tokenResult = accessTokenProvider.ValidateToken(authCookie);

                accessTokenProvider.ValidateTokenStatus(tokenResult);

                var claim = accessTokenProvider.RetriveTokenData(tokenResult, TOKEN_KEY_USERPROFILEID);

                if (!string.IsNullOrWhiteSpace(claim?.Value))
                {
                    var profileId = long.Parse(claim.Value);

                    var profileGroupService = (ProfileGroupService)Engine.Resolve<ICrudService<ProfileGroup>>();
                    profile = profileGroupService.GetAppProfileRoles(profileId).ToList();
                }

            }
            else if (bool.Parse(Engine.AppSettings["EnableAnonymousLogin"]))
            {
                var profileGroupService = (ProfileGroupService)Engine.Resolve<ICrudService<ProfileGroup>>();
                profile = profileGroupService.GetAppProfileRoles(1).ToList();
            }

            var values = new List<string>();
            roles.ForEach(role =>
            {
                if (profile.Any(r => r == role || r == "ADMIN"))
                    values.Add(role);
            });

            return values;
        }

        public IProfileGroup GetCurrentProfile()
        {
            try
            {
                if (Singleton<TestManager>.Instance?.RunningInTestScope ?? false)
                {
                    if (Singleton<CoreTestManager>.Instance.CurrentUser != null)
                    {
                        var userId = Singleton<CoreTestManager>.Instance.CurrentUser.Value;

                        return Table.Where(u => u.Id == userId).Select(u => u.Profile).FirstOrDefault();
                    }
                    else
                    {
                        return null;
                    }
                }

                if (HttpContextHelper.Current == null || HttpContextHelper.Current.Request == null) return null;

                var authCookie = HttpContextHelper.Current.Request.Cookies["Authentication"];

                if (!string.IsNullOrEmpty(authCookie))
                {
                    var tokenResult = accessTokenProvider.ValidateToken(authCookie);

                    accessTokenProvider.ValidateTokenStatus(tokenResult);

                    var claim = accessTokenProvider.RetriveTokenData(tokenResult, TOKEN_KEY_USERPROFILEID);

                    if (!string.IsNullOrWhiteSpace(claim?.Value))
                    {
                        var profileId = long.Parse(claim.Value);

                        return Engine.Resolve<IRepository<ProfileGroup>>().GetById(profileId);
                    }
                }

                if (bool.Parse(Engine.AppSettings["EnableAnonymousLogin"]))
                {
                    return Engine.Resolve<IRepository<ProfileGroup>>().GetById(1);
                }

                return null;
            }
            catch (Exception)
            {
#if DEBUG
                return _repository.Table.FirstOrDefault().Profile;
#endif
                throw;
            }
        }

        public IUser GetCurrent()
        {
            try
            {
                if (Singleton<TestManager>.Instance?.RunningInTestScope ?? false)
                {
                    if (Singleton<CoreTestManager>.Instance.CurrentUser != null)
                    {
                        return GetById(Singleton<CoreTestManager>.Instance.CurrentUser.Value);
                    }
                    else
                    {
                        return null;
                    }
                }

                if (HttpContextHelper.Current == null || HttpContextHelper.Current.Request == null) return null;

                var authCookie = HttpContextHelper.Current.Request.Cookies["Authentication"];

                if (!string.IsNullOrEmpty(authCookie))
                {
                    var tokenResult = accessTokenProvider.ValidateToken(authCookie);

                    accessTokenProvider.ValidateTokenStatus(tokenResult);

                    var claim = accessTokenProvider.RetriveTokenData(tokenResult, TOKEN_KEY_USERID);

                    if (!string.IsNullOrWhiteSpace(claim?.Value))
                    {
                        var userId = long.Parse(claim.Value);

                        return GetById(userId);
                    }
                }

                if (bool.Parse(Engine.AppSettings["EnableAnonymousLogin"]))
                {
                    return GetById(1);
                }

                return null;
            }
            catch (Exception)
            {
#if DEBUG
                return _repository.Table.FirstOrDefault();
#endif
                return null;
            }
        }

        public long? GetCurrentId()
        {
            if (Singleton<TestManager>.Instance?.RunningInTestScope ?? false)
            {
                return Singleton<CoreTestManager>.Instance.CurrentUser;
            }

            if (HttpContextHelper.Current == null || HttpContextHelper.Current.Request == null) return null;

            var authCookie = HttpContextHelper.Current.Request.Cookies["Authentication"];

            if (!string.IsNullOrEmpty(authCookie))
            {
                var tokenResult = accessTokenProvider.ValidateToken(authCookie);

                accessTokenProvider.ValidateTokenStatus(tokenResult);

                var claim = accessTokenProvider.RetriveTokenData(tokenResult, TOKEN_KEY_USERID);

                if (!string.IsNullOrWhiteSpace(claim?.Value))
                {
                    var userId = long.Parse(claim.Value);

                    return userId;
                }
            }

            if (bool.Parse(Engine.AppSettings["EnableAnonymousLogin"]))
            {
                return 1;
            }

            return null;
        }

        public long? GetCurrentProfileId()
        {
            try
            {
                if (Singleton<TestManager>.Instance?.RunningInTestScope ?? false)
                {
                    if (Singleton<CoreTestManager>.Instance.CurrentUser != null)
                    {
                        return GetById(Singleton<CoreTestManager>.Instance.CurrentUser.Value).Profile.Id;
                    }
                    else
                    {
                        return null;
                    }
                }

                if (HttpContextHelper.Current == null || HttpContextHelper.Current.Request == null) return null;

                var authCookie = HttpContextHelper.Current.Request.Cookies["Authentication"];

                if (!string.IsNullOrEmpty(authCookie))
                {
                    var tokenResult = accessTokenProvider.ValidateToken(authCookie);

                    accessTokenProvider.ValidateTokenStatus(tokenResult);

                    var claim = accessTokenProvider.RetriveTokenData(tokenResult, TOKEN_KEY_USERPROFILEID);

                    if (!string.IsNullOrWhiteSpace(claim?.Value))
                    {
                        var profileId = long.Parse(claim.Value);

                        return profileId;
                    }
                }

                if (bool.Parse(Engine.AppSettings["EnableAnonymousLogin"]))
                {
                    return 1;
                }

                return null;
            }
            catch (Exception)
            {
#if DEBUG
                return 1;
#endif
                throw;
            }
        }

        public List<long> GetCurrentUserOrgList(PortalUser currentUser = null)
        {
            long? userId = (currentUser != null) ? currentUser.Id : GetCurrentId();

            if (userId == null) return null;

            var userOrgList = redisService.Get($"UserOrgList{userId}").ToString();

            if (userOrgList == null)
            {
                var list = Table.Where(c => c.Id == userId).SelectMany(o => o.OrganizationalStructures).Select(o => o.Id).ToList();

                redisService.Set($"UserOrgList{userId}", JsonConvert.SerializeObject(list));

                return list;
            }
            else
            {
                return JsonConvert.DeserializeObject<List<long>>(userOrgList);
            }
        }

        public void ChangePass(string userName, string oldPassword, string newPassword)
        {
            ValidatePassword(newPassword);

            var encodedPass = string.IsNullOrEmpty(oldPassword) ? "" : oldPassword.EncodeSHA1();

            var currentUser = base.Table.FirstOrDefault(u => u.Login == userName && (u.Password == encodedPass || u.TempPassword == oldPassword));

            if (currentUser != null)
            {
                currentUser.ChangingPass = true;

                currentUser.Password = newPassword;

                ValidatePasswordHistory(currentUser);

                UpdatePassword(currentUser);
            }
            else
            {
                throw new BusinessException(Engine.Get("usuario_invalido"));
            }
        }

        public void ValidatePassword(string password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                if (password.Length < 8 || password.Length > 30 ||
                Regex.Match(password, @"\d+").Success == false ||
                Regex.Match(password, @"[a-z]").Success == false ||
                Regex.Match(password, @"[A-Z]").Success == false ||
                password.All(c => Char.IsLetterOrDigit(c)) == true)
                {
                    throw new BusinessException(Engine.Get("PasswordToWeak"));
                }
            }
        }
    }
}