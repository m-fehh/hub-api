using Hub.Application.Services;
using Hub.Domain.Entity;
using Hub.Domain.Interfaces;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Database.Models;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Extensions.Generate;
using Hub.Infrastructure.Redis.Cache;
using Hub.Infrastructure.Redis;
using Hub.Infrastructure.Security;
using Hub.Infrastructure.Web;
using Hub.Infrastructure;
using Hub.Shared.DataConfiguration;
using Newtonsoft.Json;
using System.Web;
using Hub.Infrastructure.TimeZone;
using Hub.Shared.Interfaces;

namespace Hub.Application.Configuration
{
    /// <summary>
    /// Implementação baseada nos cookies para obter a atual estrutura organizacional (modelo tradicional do ELOS)
    /// </summary>
    public class HubCurrentOrganizationStructure : IHubCurrentOrganizationStructure
    {
        public void Set(string id)
        {
            HttpContextHelper.Current.Response.Cookies.Append("current-organizational-structure", id);
        }

        public void SetCookieRequest(string id)
        {
            HttpContextHelper.Current.Request.Cookies.Append(new KeyValuePair<string, string>("current-organizational-structure", id));
        }

        /// <summary>
        /// Método que armazena em um cookie a URL para redirecionamento em caso de slots de implantação.
        /// No lado do cliente, haverá um redirect caso a URL acessada seja diferente da URL definida para a unidade selecionada.
        /// </summary>
        /// <param name="id"></param>
        public string GetDeploymentSlotRedirect(string id)
        {
            var deploymentSlotRedirectUrl = "-";

            if (bool.Parse(Engine.AppSettings["HasStagingSlots"] ?? "false") == true)
            {
                long o;

                if (long.TryParse(id, out o))
                {
                    deploymentSlotRedirectUrl = Engine.Resolve<OrganizationalStructureService>().GetConfigByName(o, "DeploymentSlotRedirect");
                }
            }

            if (deploymentSlotRedirectUrl != "-")
            {
                var auth = HttpContextHelper.Current.Request.Cookies["Authentication"];

                if (string.IsNullOrEmpty(auth) == false)
                {
                    HttpContextHelper.Current.Response.Cookies.Append("slot-routing-session", HttpUtility.UrlEncode(Engine.Resolve<IStringEncrypter>().Encrypt(auth)));
                }

            }

            return deploymentSlotRedirectUrl;
        }

        public string Get()
        {
            try
            {
                if (Singleton<TestManager>.Instance?.RunningInTestScope ?? false)
                {
                    var orgService = Engine.Resolve<OrganizationalStructureService>();

                    switch (Singleton<CoreTestManager>.Instance.CurrentScope)
                    {
                        case ETestOrganizationalScope.Leaf:
                            return orgService.Table.Where(c => c.IsLeaf && !c.Inactive).Select(c => c.Id).FirstOrDefault().ToString();

                        case ETestOrganizationalScope.Domain:
                            return orgService.Table.Where(c => c.IsDomain && !c.Inactive).Select(c => c.Id).FirstOrDefault().ToString();

                        case ETestOrganizationalScope.Root:
                            return orgService.Table.Where(c => c.IsRoot && !c.Inactive).Select(c => c.Id).FirstOrDefault().ToString();

                        default:
                            break;
                    }
                }

                var userId = Engine.Resolve<ISecurityProvider>().GetCurrentId();

                if (userId == null) return null;

                var cookie = HttpContextHelper.Current.Request.Cookies["current-organizational-structure"];

                if (cookie == null)
                {
                    var current = Engine.Resolve<IUserSettingManager>().GetSetting("current-organizational-structure");

                    Set(current);

                    return current;
                }

                long l;

                if (long.TryParse(cookie, out l))
                {
                    var redisService = Engine.Resolve<IRedisService>();

                    List<long> UsersOrgStructs = null;

                    var UsersOrgStructsString = redisService.Get($"UserOrgList{userId}").ToString();

                    if (!string.IsNullOrEmpty(UsersOrgStructsString))
                    {
                        UsersOrgStructs = JsonConvert.DeserializeObject<List<long>>(UsersOrgStructsString);
                    }
                    else
                    {
                        UsersOrgStructs = UpdateUser(userId.Value);
                    }

                    if (!UsersOrgStructs.Any(o => o == l))
                    {
                        var defaultOrgStructure = Engine.Resolve<IRepository<PortalUser>>().Table.Where(u => u.Id == userId.Value).Select(u => u.DefaultOrgStructure.Id).FirstOrDefault();

                        Set(defaultOrgStructure.ToString());

                        return defaultOrgStructure.ToString();
                    }

                    return cookie;
                }
                else
                {
                    var defaultOrgStructure = Engine.Resolve<IRepository<PortalUser>>().Table.Where(u => u.Id == userId.Value).Select(u => u.DefaultOrgStructure.Id).FirstOrDefault();

                    Set(defaultOrgStructure.ToString());

                    return defaultOrgStructure.ToString();
                }
            }
            catch (Exception)
            {
#if DEBUG
                var orgService = Engine.Resolve<OrganizationalStructureService>();

                if (orgService.GetCurrentOrgStructureIfNull() != null) return orgService.GetCurrentOrgStructureIfNull().ToString();
#endif
                throw;
            }
        }

        public List<long> UpdateUser(long userid)
        {
            var redisService = Engine.Resolve<IRedisService>();

            var list = Engine.Resolve<ICrudService<PortalUser>>().Table.Where(c => c.Id == userid).SelectMany(c => c.OrganizationalStructures).Select(o => o.Id).ToList();

            redisService.Set($"UserOrgList{userid}", JsonConvert.SerializeObject(list));

            return list;
        }

        public string GetCurrentDomain(string structId = null)
        {
            if (string.IsNullOrWhiteSpace(structId))
            {
                structId = Engine.Resolve<IHubCurrentOrganizationStructure>().Get();

                var localcached = Engine.Resolve<PortalCache>().Get().CurrentDomain;

                if (!string.IsNullOrEmpty(localcached)) return localcached;

            }

            var redisService = Engine.Resolve<IRedisService>();

            var cached = redisService.Get($"CurrentDomain{structId}").ToString();

            if (!string.IsNullOrEmpty(cached))
            {
                try
                {
                    Engine.Resolve<PortalCache>().Get().CurrentDomain = cached;
                }
                catch (Exception) { }

                return cached;
            }

            var fromDb = GetCurrentDomainFromDb(structId);

            redisService.Set($"CurrentDomain{structId}", fromDb, TimeSpan.FromHours(3));

            try
            {
                Engine.Resolve<PortalCache>().Get().CurrentDomain = fromDb;
            }
            catch (Exception) { }

            return fromDb;
        }


        private string GetCurrentDomainFromDb(string structId = null)
        {
            var structService = Engine.Resolve<ICrudService<OrganizationalStructure>>();
            var longStructId = long.Parse(structId);

            var structure = structService.Table.Where(o => o.Id == longStructId).Select(o => new { o.IsDomain, FatherId = (long?)o.Father.Id }).FirstOrDefault();

            if (structure == null)
            {
                throw new Exception(Engine.Get("OrganizationalStructureNotFound"));
            }

            if (structure.IsDomain) return structId;

            if (structure.FatherId != null)
            {
                // Caso a unidade não possuir Central
                if (structService.Table.Any(c => c.Id == structure.FatherId && c.IsRoot))
                    return structId;

                return GetCurrentDomainFromDb(structure.FatherId.ToString());
            }
            else
            {
                return null;
            }
        }

        public OrganizationalStructure GetCurrentRoot()
        {
            return Engine.Resolve<IRepository<OrganizationalStructure>>().CacheableTable.FirstOrDefault(o => o.IsRoot == true);
        }

        public long? GetCurrentRootId()
        {
            return Engine.Resolve<IRepository<OrganizationalStructure>>().CacheableTable.Where(o => o.IsRoot == true).Select(o => (long?)o.Id).FirstOrDefault();
        }
    }

    /// <summary>
    /// Implementação da interface do FMK.Core para obter a atual estrutura organizacional (utiliza o <see cref="ElosCurrentOrganizationStructure"/> como provedor)
    /// </summary>
    public class CurrentOrganizationStructure : ICurrentOrganizationStructure
    {
        private readonly IRepository<OrganizationalStructure> repository;
        private readonly IHubCurrentOrganizationStructure hubCurrentOrganizationStructure;

        public CurrentOrganizationStructure(
            IRepository<OrganizationalStructure> repository,
            IHubCurrentOrganizationStructure hubCurrentOrganizationStructure)
        {
            this.repository = repository;
            this.hubCurrentOrganizationStructure = hubCurrentOrganizationStructure;
        }

        OrganizationalStructureVM GetById(long id)
        {
            Func<long, OrganizationalStructureVM> fn = (orgId) =>
            {
                return repository.Table.Where(o => o.Id == id)
                .Select(o => new OrganizationalStructureVM
                {
                    Id = o.Id,
                    Abbrev = o.Abbrev,
                    Description = o.Description,
                    IsDomain = o.IsDomain,
                    IsLeaf = o.IsLeaf,
                    IsRoot = o.IsRoot,
                    Inactive = o.Inactive,
                    Father_Id = o.Father.Id,
                    Father_Description = o.Father.Description
                }).FirstOrDefault();
            };

            return Engine.Resolve<CacheManager>().CacheAction(() => fn(id));
        }

        public TimeZoneInfo GetTimezone(long id)
        {
            Func<long, string> fn = (orgId) =>
            {
                return Engine.Resolve<IRepository<Establishment>>().Table.Where(e => e.OrganizationalStructure.Id == id).Select(e => e.Timezone).FirstOrDefault();
            };

            var timezone = Engine.Resolve<CacheManager>().CacheAction(() => fn(id));

            return TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }

        public OrganizationalStructureVM GetCurrent()
        {
            var currentId = hubCurrentOrganizationStructure.Get();

            if (string.IsNullOrEmpty(currentId)) return null;

            return GetById(long.Parse(currentId));
        }

        public OrganizationalStructureVM GetCurrentDomain(long? structId = null)
        {
            var current = GetCurrent();

            var org = structId == null ? current : current.Id == structId ? current : GetById(structId.Value);

            if (org.IsDomain)
            {
                return org;
            }
            if (org.IsRoot)
            {
                return null;
            }

            return GetById(org.Father_Id.Value);
        }

        public OrganizationalStructureVM GetCurrentRoot()
        {
            var domain = GetCurrentDomain();

            if (domain == null) return null;

            return GetById(domain.Father_Id.Value);
        }

        public TimeZoneInfo GetCurrentTimezone()
        {
            var currentId = hubCurrentOrganizationStructure.Get();

            if (string.IsNullOrEmpty(currentId)) return null;

            return GetTimezone(long.Parse(currentId));
        }

        public OrganizationalStructureVM Set(long id)
        {
            hubCurrentOrganizationStructure.Set(id.ToString());

            return GetById(id);
        }

        public void Set(OrganizationalStructureVM org)
        {
            hubCurrentOrganizationStructure.Set(org.Id.ToString());
        }

        public void SetByCookieData(string cookieData)
        {
            if (string.IsNullOrEmpty(cookieData)) return;

            var model = JsonConvert.DeserializeObject<OrganizationalStructureVM>(cookieData);

            hubCurrentOrganizationStructure.Set(model.Id.ToString());
        }
    }
}
