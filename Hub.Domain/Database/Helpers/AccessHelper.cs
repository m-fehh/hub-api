using Hub.Domain.Entity;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure;

namespace Hub.Domain.Database.Helpers
{
    /// <summary>
    /// Classe auxiliar para criar submenus
    /// </summary>
    public static class AccessHelper
    {
        public static void CreateAccessRules(AccessRuleParameters parameters)
        {
            CreateAccessRules(parameters, out long idAccessCB, out long idAccessVis);
        }

        public static void CreateAccessRules(AccessRuleParameters parameters, out long idAccessCB, out long idAccessVis)
        {
            var accessRuleRepository = Engine.Resolve<ICrudService<AccessRule>>();

            idAccessCB = accessRuleRepository.Table.Where(a => a.KeyName == parameters.ParentKeyName).Select(c => c.Id).FirstOrDefault();

            var idAccessRoot = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessCB },
                Description = parameters.Description,
                KeyName = $"CB_{parameters.KeyName}"
            });
            idAccessVis = accessRuleRepository.Insert(new AccessRule()
            {
                Parent = new AccessRule() { Id = idAccessRoot },
                Description = "View",
                KeyName = $"CB_{parameters.KeyName}_Vis"
            });
            if (parameters.IsInsertEnable)
            {
                accessRuleRepository.Insert(new AccessRule()
                {
                    Parent = new AccessRule() { Id = idAccessRoot },
                    Description = "Insert",
                    KeyName = $"CB_{parameters.KeyName}_Ins"
                });
            }
            if (parameters.IsUpdateEnable)
            {
                accessRuleRepository.Insert(new AccessRule()
                {
                    Parent = new AccessRule() { Id = idAccessRoot },
                    Description = "Edit",
                    KeyName = $"CB_{parameters.KeyName}_Upd"
                });
            }
            if (parameters.IsDeleteEnable)
            {
                accessRuleRepository.Insert(new AccessRule()
                {
                    Parent = new AccessRule() { Id = idAccessRoot },
                    Description = "Delete",
                    KeyName = $"CB_{parameters.KeyName}_Del"
                });
            }
            if (parameters.IsExportEnable)
            {
                accessRuleRepository.Insert(new AccessRule()
                {
                    Parent = new AccessRule() { Id = idAccessRoot },
                    Description = "Export",
                    KeyName = $"CB_{parameters.KeyName}_Exp"
                });
            }
        }

        public static void CreateSubmenu(SubmenuParameters parameters, bool withMenu = true)
        {
            CreateAccessRules(parameters.AccessRuleParameters, out long idAccessCB, out long idAccessVis);

            #region Menu

            if (withMenu)
            {
                var menuRepository = Engine.Resolve<IRepository<PortalMenu>>();
                var menuItemRepository = Engine.Resolve<IRepository<PortalMenuItem>>();

                var idMenu = menuRepository.Table.Where(m => m.Name == parameters.Menu).Select(c => c.Id).FirstOrDefault();

                var idMenuItemCB = menuItemRepository.Table.Where(m => m.Rule.Id == idAccessCB).Select(c => c.Id).FirstOrDefault();

                menuItemRepository.Insert(new PortalMenuItem()
                {
                    Menu = new PortalMenu() { Id = idMenu },
                    Parent = new PortalMenuItem() { Id = idMenuItemCB },
                    Rule = new AccessRule() { Id = idAccessVis },
                    Url = parameters.Url,
                    Order = parameters.ColOrder
                });
            }

            #endregion
        }
    }

    /// <summary>
    /// Parâmetros para inclusão de registros nas tabelas AccessRule e PortalMenuItem através da classe auxiliar AccessHelper
    /// </summary>
    public class SubmenuParameters
    {
        /// <summary>
        /// Equivalente a tabela PortalMenu.Name
        /// </summary>
        public string Menu { get; set; } = "main-portal";

        /// <summary>
        /// Usada para gerar a cadeia de AccessRule. Exemplo: ao usar "XYZ" serão gerados (por padrão) CB_XYZ, CB_XYZ_Vis, CB_XYZ_Ins, CB_XYZ_Upd, CB_XYZ_Del e CB_XYZ_Exp
        /// </summary>
        public AccessRuleParameters AccessRuleParameters { get; set; }

        /// <summary>
        /// Url da controller
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Posição do submenu dentro do menu
        /// </summary>
        public int ColOrder { get; set; }

        public SubmenuParameters(AccessRuleParameters accessRuleParameters, string url, int colOrder)
        {
            AccessRuleParameters = accessRuleParameters;
            Url = url;
            ColOrder = colOrder;
        }
    }

    /// <summary>
    /// Parâmetros para inclusão de registros na tabela AccessRule através da classe auxiliar AccessHelper
    /// </summary>
    public class AccessRuleParameters
    {
        /// <summary>
        /// Usada para gerar a cadeia de AccessRule. Exemplo: ao usar "XYZ" serão gerados (por padrão) CB_XYZ, CB_XYZ_Vis, CB_XYZ_Ins, CB_XYZ_Upd, CB_XYZ_Del e CB_XYZ_Exp
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// Equivalente a tabela AccessRule.KeyName. Exemplo: para adicionar um submenu ao menu "Clientes", passe "CL"
        /// </summary>
        public string ParentKeyName { get; set; }

        /// <summary>
        /// Quando falso, não insere AccessRule CB_XYZ_Ins
        /// </summary>
        public bool IsInsertEnable { get; set; } = true;

        /// <summary>
        /// Quando falso, não insere AccessRule CB_XYZ_Upd
        /// </summary>
        public bool IsUpdateEnable { get; set; } = true;

        /// <summary>
        /// Quando falso, não insere AccessRule CB_XYZ_Del
        /// </summary>
        public bool IsDeleteEnable { get; set; } = true;

        /// <summary>
        /// Quando falso, não insere AccessRule CB_XYZ_Exp
        /// </summary>
        public bool IsExportEnable { get; set; } = true;

        /// <summary>
        /// Opcional, para usar com Engine.Get
        /// </summary>
        public string Description { get; set; }

        public AccessRuleParameters(string keyName, string parentKeyName, string description = null)
        {
            KeyName = keyName;
            ParentKeyName = parentKeyName;
            Description = description ?? keyName;
        }
    }
}
