using Hub.Infrastructure.Database;
using Hub.Infrastructure.Web;
using Hub.Infrastructure;
using Hub.Infrastructure.Exceptions;
using Hub.Shared.Models.VMs;
using Dapper;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure.Redis;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Hub.Application.Services
{
    public class LoginService
    {
        private readonly DatabaseConnectionProvider connProvider;
        private readonly IAccessTokenProvider _tokenService;

        public LoginService(DatabaseConnectionProvider connProvider)
        {
            this.connProvider = connProvider;

            _tokenService = Engine.Resolve<IAccessTokenProvider>();
        }

        public string Login(LoginVM model)
        {
            if (string.IsNullOrEmpty(model.Username))
            {
                throw new BusinessException(Engine.Get("UsernameCannotBeNull"));
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                throw new BusinessException(Engine.Get("PasswordCannotBeNull"));
            }

            return CreateToken(model);
        }

        string CreateToken(LoginVM model)
        {
            var connection = connProvider.GetConnection();
            var schema = connProvider.GetSchema();

            var password = model.Password.EncodeSHA1();

            var user = connection.QueryFirstOrDefault<LoginResponseVM>(
                $@"SELECT 
                    p.Id,
                    p.Name,
                    p.Email,
                    p.ProfileId,
                    g.Administrator,
                    p.DefaultOrgStructureId,
                    p.Inactive,
                    g.AllowMultipleAccess
                FROM {schema}.PortalUser p 
                JOIN {schema}.ProfileGroup g ON g.Id = p.ProfileId
                WHERE p.Login = @Username AND p.Password = @Password", new { Username = model.Username, Password = password });

            if (user == null)
            {
                throw new BusinessException(Engine.Get("InvalidLogin"));
            }

            if (user.Inactive)
            {
                throw new BusinessException(Engine.Get("UserInactive"));
            }

            var dateOfLastChangePassword = connection.QueryFirstOrDefault<DateTime?>($@"SELECT p.ExpirationUTC FROM {schema}.PortalUserPassHistory p WHERE UserId = @Id AND Password = @Password ORDER BY p.ExpirationUTC DESC", new { Id = user.Id, Password = password });

            if (dateOfLastChangePassword.HasValue && dateOfLastChangePassword.Value < DateTime.UtcNow)
            {
                throw new BusinessException(Engine.Get("PasswordExpired"));
            }

            connection.Execute(@$"UPDATE {schema}.PortalUser SET TempPassword = NULL, LastAccessDate = @today, LastUpdateUTC = @updateDate WHERE Id = @Id",
                new
                {
                    today = DateTime.Now,
                    updateDate = DateTime.UtcNow,
                    Id = user.Id
                });

            return CreateToken(user);
        }

        string CreateToken(LoginResponseVM user)
        {
            var connection = connProvider.GetConnection();
            var schema = connProvider.GetSchema();

            var redisService = Engine.Resolve<IRedisService>();

            var cacheKey = $"UpdatedUserAccess{user.Id}";

            var userToRevoke = redisService.Get(cacheKey).ToString();

            if (!string.IsNullOrWhiteSpace(userToRevoke))
            {
                redisService.Delete(cacheKey);
            }

            var list = connection.Query<long>($@"SELECT p.StructureId FROM {schema}.PortalUser_OrgStructure p WHERE p.UserId = @Id", user);

            redisService.Set($"UserOrgList{user.Id}", JsonConvert.SerializeObject(list), TimeSpan.FromDays(7));

            double tokenExpirationTimeInMinutes = double.TryParse(Engine.AppSettings["Hub-auth-token-expiration-time"], out var tokenExpirationTime) ? tokenExpirationTime : 180;

            var token = _tokenService.GenerateToken(new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("admin", user.Administrator.ToString()),
                        new Claim("profileId", user.ProfileId.ToString()),
                        new Claim("defaultOrgStructureId", user.DefaultOrgStructureId?.ToString() ?? ""),
                        new Claim("allowMultipleAccess", user.AllowMultipleAccess.ToString()),
                    }, expiryInMinutes: tokenExpirationTimeInMinutes);

            return token;
        }

        public string RefreshToken(string token)
        {
            var loginData = ValidateToken(token);

            return CreateToken(loginData.Id.Value);
        }

        string CreateToken(long userId)
        {
            var connection = connProvider.GetConnection();
            var schema = connProvider.GetSchema();

            var user = connection.QueryFirstOrDefault<LoginResponseVM>(
                $@"SELECT 
                    p.Id,
                    p.Name,
                    p.Email,
                    p.ProfileId,
                    g.Administrator,
                    p.DefaultOrgStructureId,
                    p.Inactive,
                    g.AllowMultipleAccess
                FROM {schema}.PortalUser p 
                JOIN {schema}.ProfileGroup g ON g.Id = p.ProfileId
                WHERE p.Id = @Id", new { Id = userId });

            if (user == null)
            {
                throw new BusinessException(Engine.Get("InvalidLogin"));
            }

            return CreateToken(user);
        }

        public LoginResponseVM ValidateToken(string token)
        {
            ClaimsPrincipal principals;

            try
            {
                var result = _tokenService.ValidateToken(token);

                _tokenService.ValidateTokenStatus(result);

                principals = result.Principal;
            }
            catch (ArgumentException ex)
            {
                throw new BusinessException(Engine.Get("ValidationTokenIsInvalid"));
            }
            catch (SecurityTokenExpiredException stex)
            {
                throw new BusinessException(Engine.Get("ValidationTokenIsExpired"));
            }

            var defaultOrgStructureId = principals.Claims.FirstOrDefault(c => c.Type == "defaultOrgStructureId")?.Value;

            var response = new LoginResponseVM
            {
                Id = long.Parse(principals.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value),
                Name = principals.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                Email = principals.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                ProfileId = long.Parse(principals.Claims.First(c => c.Type == "profileId" || c.Type == "userProfileId").Value),
                Administrator = bool.Parse(principals.Claims.FirstOrDefault(c => c.Type == "admin")?.Value ?? "false"),
                DefaultOrgStructureId = !string.IsNullOrEmpty(defaultOrgStructureId) ? long.Parse(defaultOrgStructureId) : null
            };

            return response;
        }
    }
}
