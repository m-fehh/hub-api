using Hub.Domain.Database.Runner;
using Hub.Domain.Entity;
using Hub.Infrastructure;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Extensions;

namespace Hub.Domain.Developments.Startups._2024
{
    public class StatupData202412062350 : IStartupData
    {
        public void Execute() 
        {
            if (Singleton<ISchemaNameProvider>.Instance.TenantName() == "system") return;

            var profileGroupRepository = Engine.Resolve<IRepository<ProfileGroup>>();
            var structureRepository = Engine.Resolve<IRepository<OrganizationalStructure>>();
            var userRepository = Engine.Resolve<IRepository<PortalUser>>();

            #region Organizational Structure

            var defaultOrganizationalStructure = new OrganizationalStructure()
            {
                Abbrev = "GRP",
                Description = "Raiz (Renomeie)",
                IsRoot = true,
                IsLeaf = false,
                Inactive = false,
                CreationUTC = DateTime.UtcNow,
                LastUpdateUTC = DateTime.UtcNow
            };

            structureRepository.Insert(defaultOrganizationalStructure);

            #endregion

            #region Profile Group

            var idProfileAdmin = profileGroupRepository.Insert(new ProfileGroup()
            {
                Name = "Administrador",
                Administrator = true,
                OwnerOrgStruct = defaultOrganizationalStructure,
            });

            #endregion

            #region Users

            var user = new PortalUser()
            {
                Name = "Administrador",
                Password = "123".EncodeSHA1(),
                Login = "admin",
                Email = "admin@hub.com.br",
                Profile = profileGroupRepository.GetById(idProfileAdmin),
                OwnerOrgStruct = defaultOrganizationalStructure,
                OrganizationalStructures = new HashSet<OrganizationalStructure>() { defaultOrganizationalStructure }
            };

            userRepository.Insert(user);

            #endregion
        }

        public long Order
        {
            get { return 202412062350; }
        }
    }
}
