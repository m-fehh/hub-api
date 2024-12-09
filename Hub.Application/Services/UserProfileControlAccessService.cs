using Hub.Infrastructure.Redis;
using Hub.Infrastructure;
using Hub.Shared.Interfaces;
using Newtonsoft.Json;
using Hub.Domain.Entity;
using Hub.Infrastructure.Exceptions;
using Hub.Shared.Models.VMs;

namespace Hub.Application.Services
{
    /// <summary>
    /// Serviço responsável por armazenamento do token do navegador do usuário
    /// e realizar a validação do mesmo. 
    /// </summary>
    public class UserProfileControlAccessService : IUserProfileControlAccessService
    {
        private readonly IRedisService _redisService;
        public UserProfileControlAccessService(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public virtual void Save(ProfileControlAccessVM profileControlAccess, bool allowMultipleAccess = false)
        {
            if (!allowMultipleAccess)
            {
                var redisData = Get(profileControlAccess);

                if (redisData == null || string.IsNullOrEmpty(redisData.Token) || redisData.Token != profileControlAccess.Token)
                    Set(profileControlAccess);
            }
            else
            {
                var redisKey = profileControlAccess.ToString();
                _redisService.Delete(redisKey);
            }
        }

        protected virtual ProfileControlAccessVM Get(ProfileControlAccessVM profileControlAccess)
        {
            var redisKey = profileControlAccess.ToString();
            var json = _redisService.Get(redisKey).ToString();

            if (!string.IsNullOrEmpty(json))
                return JsonConvert.DeserializeObject<ProfileControlAccessVM>(json);

            return null;
        }

        protected virtual void Set(ProfileControlAccessVM profileControlAccess)
        {
            _redisService.Set(profileControlAccess.ToString(), JsonConvert.SerializeObject(profileControlAccess));
        }

        /// <summary>
        /// Validar se o token armazenado no redis é diferente do navegador.
        /// </summary>
        /// <param name="parameter">Objeto que representa o token</param>
        /// <exception cref="AuthenticationException"></exception>
        public virtual void Validate(ProfileControlAccessVM parameter)
        {
            var cacheToken = Get(parameter);

            if (cacheToken == null)
            {
                var profileAllowMultipleAccess = Engine.Resolve<UserService>().Get(w => w.Id == parameter.UserId, s => ((ProfileGroup)s.Profile).AllowMultipleAccess, 1, true).FirstOrDefault();
                if (profileAllowMultipleAccess == false)
                {
                    this.Save(parameter, profileAllowMultipleAccess);
                }
            }

            if (cacheToken != null && cacheToken.Token != parameter.Token && string.IsNullOrEmpty(parameter.Token) == false)
            {
                throw new AuthenticationException(Engine.Get("ErrorAlreadyLogin"));
            }
        }
    }
}
