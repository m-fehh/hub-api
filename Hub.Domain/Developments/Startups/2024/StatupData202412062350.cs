using Hub.Domain.Database.Runner;
using Hub.Domain.Database.Services;
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

            var userRepository = Engine.Resolve<IRepository<PortalUser>>();
            var profileGroupRepository = Engine.Resolve<IRepository<ProfileGroup>>();
            var accessRuleRepository = Engine.Resolve<AccessRuleService>();
            var menuRepository = Engine.Resolve<IRepository<PortalMenu>>();
            var menuItemRepository = Engine.Resolve<IRepository<PortalMenuItem>>();
            var structureRepository = Engine.Resolve<IRepository<OrganizationalStructure>>();


            #region Access Rules

            var idAccessCB = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = null,
                Description = "BasicRegistration",
                KeyName = "CB"
            });

            #region User

            var idAccessUsu = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessCB },
                Description = "PortalUser",
                KeyName = "CB_User"
            });
            var idAccessUsuVis = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessUsu },
                Description = "View",
                KeyName = "CB_User_Vis"
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessUsu },
                Description = "Insert",
                KeyName = "CB_User_Ins"
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessUsu },
                Description = "Edit",
                KeyName = "CB_User_Upd"
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessUsu },
                Description = "Delete",
                KeyName = "CB_User_Del"
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessUsu },
                Description = "Export",
                KeyName = "CB_User_Exp"
            });

            #endregion

            #region Profile

            var idAccessProfileGroup = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessCB },
                Description = "ProfileGroup",
                KeyName = "CB_ProfileGroup"
            });
            var idAccessProfileVis = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessProfileGroup },
                Description = "View",
                KeyName = "CB_ProfileGroup_Vis"
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessProfileGroup },
                Description = "Insert",
                KeyName = "CB_ProfileGroup_Ins"
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessProfileGroup },
                Description = "Edit",
                KeyName = "CB_ProfileGroup_Upd"
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessProfileGroup },
                Description = "Delete",
                KeyName = "CB_ProfileGroup_Del"
            });

            #endregion

            #region Organizational Structure

            var idAccessOrgStruc = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessCB },
                Description = "OrganizationalStructure",
                KeyName = "CB_OrganizationalStructure"
            });
            var idAccessOrgStrucVis = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessOrgStruc },
                Description = "View",
                KeyName = "CB_OrganizationalStructure_Vis"
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessOrgStruc },
                Description = "Insert",
                KeyName = "CB_OrganizationalStructure_Ins",
                ForAdministratorOnly = true
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessOrgStruc },
                Description = "Edit",
                KeyName = "CB_OrganizationalStructure_Upd",
                ForAdministratorOnly = true
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessOrgStruc },
                Description = "Delete",
                KeyName = "CB_OrganizationalStructure_Del",
                ForAdministratorOnly = true
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessOrgStruc },
                Description = "Export",
                KeyName = "CB_OrganizationalStructure_Exp"
            });

            #endregion

            //#region AddressCountry

            //var idAccessAddressCountry = accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessCB },
            //    Description = "AddressCountry",
            //    KeyName = "CB_AddressCountry"
            //});
            //var idAccessAddressCountryVis = accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressCountry },
            //    Description = "View",
            //    KeyName = "CB_AddressCountry_Vis"
            //});
            //accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressCountry },
            //    Description = "Insert",
            //    KeyName = "CB_AddressCountry_Ins"
            //});
            //accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressCountry },
            //    Description = "Edit",
            //    KeyName = "CB_AddressCountry_Upd"
            //});
            //accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressCountry },
            //    Description = "Delete",
            //    KeyName = "CB_AddressCountry_Del"
            //});

            //#endregion

            //#region AddressState

            //var idAccessAddressState = accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessCB },
            //    Description = "AddressState",
            //    KeyName = "CB_AddressState"
            //});
            //var idAccessAddressStateVis = accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressState },
            //    Description = "View",
            //    KeyName = "CB_AddressState_Vis"
            //});
            //accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressState },
            //    Description = "Insert",
            //    KeyName = "CB_AddressState_Ins"
            //});
            //accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressState },
            //    Description = "Edit",
            //    KeyName = "CB_AddressState_Upd"
            //});
            //accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressState },
            //    Description = "Delete",
            //    KeyName = "CB_AddressState_Del"
            //});

            //#endregion

            //#region AddressCity

            //var idAccessAddressCity = accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessCB },
            //    Description = "AddressCity",
            //    KeyName = "CB_AddressCity"
            //});
            //var idAccessAddressCityVis = accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressCity },
            //    Description = "View",
            //    KeyName = "CB_AddressCity_Vis"
            //});
            //accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressCity },
            //    Description = "Insert",
            //    KeyName = "CB_AddressCity_Ins"
            //});
            //accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressCity },
            //    Description = "Edit",
            //    KeyName = "CB_AddressCity_Upd"
            //});
            //accessRuleRepository.Insert(new AccessRule()
            //{
            //    Parent = new AccessRule() { Id = idAccessAddressCity },
            //    Description = "Delete",
            //    KeyName = "CB_AddressCity_Del"
            //});

            //#endregion

            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = null,
                Description = "AdministrationSchedulerJobs",
                KeyName = "HF"
            });

            #region Log

            var idAccessSIS = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = null,
                Description = "System",
                KeyName = "SIS"
            });

            var idAccessLog = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessSIS },
                Description = "Log",
                KeyName = "SIS_Log"
            });
            var idAccessLogVis = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessLog },
                Description = "View",
                KeyName = "SIS_Log_Vis"
            });
            accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessLog },
                Description = "Export",
                KeyName = "SIS_Log_Exp"
            });

            #endregion

            #endregion

            #region Menu

            var idMenu = menuRepository.Insert(new PortalMenu()
            {
                Name = "main-portal"
            });

            var idMenuItemCB = menuItemRepository.Insert(new PortalMenuItem()
            {
                Menu = new PortalMenu() { Id = idMenu },
                Parent = null,
                Rule = new AccessRule() { Id = idAccessCB },
                IconName = "fa fa-copy"
            });

            menuItemRepository.Insert(new PortalMenuItem()
            {
                Menu = new PortalMenu() { Id = idMenu },
                Parent = new PortalMenuItem() { Id = idMenuItemCB },
                Rule = new AccessRule() { Id = idAccessUsuVis },
                Url = "~/User/Index"
            });

            menuItemRepository.Insert(new PortalMenuItem()
            {
                Menu = new PortalMenu() { Id = idMenu },
                Parent = new PortalMenuItem() { Id = idMenuItemCB },
                Rule = new AccessRule() { Id = idAccessProfileVis },
                Url = "~/ProfileGroup/Index"
            });

            menuItemRepository.Insert(new PortalMenuItem()
            {
                Menu = new PortalMenu() { Id = idMenu },
                Parent = new PortalMenuItem() { Id = idMenuItemCB },
                Rule = new AccessRule() { Id = idAccessOrgStrucVis },
                Url = "~/OrganizationalStructure/Index"
            });

            //menuItemRepository.Insert(new PortalMenuItem()
            //{
            //    Menu = new PortalMenu() { Id = idMenu },
            //    Parent = new PortalMenuItem() { Id = idMenuItemCB },
            //    Rule = new AccessRule() { Id = idAccessAddressCityVis },
            //    Url = "~/AddressCity/Index"
            //});

            var idMenuItemSIS = menuItemRepository.Insert(new PortalMenuItem()
            {
                Menu = new PortalMenu() { Id = idMenu },
                Parent = null,
                Rule = new AccessRule() { Id = idAccessSIS },
                IconName = "fa fa-cogs"
            });

            menuItemRepository.Insert(new PortalMenuItem()
            {
                Menu = new PortalMenu() { Id = idMenu },
                Parent = new PortalMenuItem() { Id = idMenuItemSIS },
                Rule = new AccessRule() { Id = idAccessLogVis },
                Url = "~/Log/Index"
            });


            #endregion

            #region Organizational Structure

            var defaultOrganizationalStructure = new OrganizationalStructure()
            {
                Abbrev = "GRP",
                Description = "Grupo Corporativo",
                IsRoot = true,
                IsLeaf = false
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
                DefaultOrgStructure = defaultOrganizationalStructure,
                OwnerOrgStruct = defaultOrganizationalStructure,
                OrganizationalStructures = new HashSet<OrganizationalStructure>() { defaultOrganizationalStructure }
            };

            userRepository.Insert(user);

            Engine.Resolve<IRepository<PortalUserSetting>>().Insert(new PortalUserSetting()
            {
                PortalUser = user,
                Name = "current-organizational-structure",
                Value = defaultOrganizationalStructure.Id.ToString()
            });

            #endregion


            //var arParameters = new AccessRuleParameters("Product", "PR") { IsUpdateEnable = false };
            //var submenuParameters = new SubmenuParameters(arParameters, "~/Product/Index", 8);
            //AccessHelper.CreateSubmenu(submenuParameters);
        }

        public long Order
        {
            get { return 202412062350; }
        }
    }
}
