using FluentMigrator;
using Hub.Domain.Developments.Migrations._2024;
using Hub.Infrastructure;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Shared.Interfaces.MultiTenant;

namespace Hub.Domain.Database.Runner
{
    public static class ForeignKeyMap
    {
        public static void Map(MigrationBase migration)
        {
            var schema = "sch" + Engine.Resolve<ITenantManager>().GetInfo().Id;

            if (migration == null || migration.GetType() == typeof(Migration202412062300))
            {
                // OrganizationalStructure - OrganizationalStructure
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_OrgStructure_Self", "OrganizationalStructure", "FatherId", "OrganizationalStructure", "Id");

                // PortalUser_OrgStructure - OrganizationalStructure / PortalUser
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_PUser_OrgStruc", "PortalUser_OrgStructure", "UserId", "PortalUser", "Id", "OrganizationalStructure");
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_OrgStruc_PUser", "PortalUser_OrgStructure", "StructureId", "OrganizationalStructure", "Id", "PortalUser");


                // PortalUser - Person / ProfileGroup / OrganizationalStructure
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_PortalUser_Person", "PortalUser", "PersonId", "Person", "Id");
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_PortalUser_ProfileGroup", "PortalUser", "ProfileId", "ProfileGroup", "Id");
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_User_OwnerOrgStruct", "PortalUser", "OwnerOrgStructId", "OrganizationalStructure", "Id");

                // AccessRule - AccessRule
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_AccessRule_Parent", "AccessRule", "ParentId", "AccessRule", "Id");

                // ProfileGroup_Rule - AccessRule / ProfileGroup
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_AccessR_ProfG_Rule", "ProfileGroup_Rule", "AccessRuleId", "AccessRule", "Id", "ProfileGroup");
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_ProfG_ProfG_Rule", "ProfileGroup_Rule", "ProfileGroupId", "ProfileGroup", "Id", "AccessRule");

                // ProfileGroup - OrganizationalStructure
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_ProfGrp_OwnerOrgStruct", "ProfileGroup", "OwnerOrgStructId", "OrganizationalStructure", "Id");
            }
        }
    }
}
