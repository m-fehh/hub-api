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
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_PortalUser_DefaultOrgStruct", "PortalUser", "DefaultOrgStructureId", "OrganizationalStructure", "Id");

                // Person - OrganizationalStructure
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_Person_OrgStructure", "Person", "OwnerOrgStructId", "OrganizationalStructure", "Id");

                // AccessRule - AccessRule
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_AccessRule_Parent", "AccessRule", "ParentId", "AccessRule", "Id");

                // ProfileGroup_Rule - AccessRule / ProfileGroup
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_AccessR_ProfG_Rule", "ProfileGroup_Rule", "AccessRuleId", "AccessRule", "Id", "ProfileGroup");
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_ProfG_ProfG_Rule", "ProfileGroup_Rule", "ProfileGroupId", "ProfileGroup", "Id", "AccessRule");

                // ProfileGroup - OrganizationalStructure
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_ProfGrp_OwnerOrgStruct", "ProfileGroup", "OwnerOrgStructId", "OrganizationalStructure", "Id");

                // PortalUserPassHistory - PortalUser
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_PortalUserPassHistory_PortalUser", "PortalUserPassHistory", "UserId", "PortalUser", "Id");

                // PortalUserSetting - PortalUser
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_PortalUserSet_PUser", "PortalUserSetting", "PortalUserId", "PortalUser", "Id");

                // Person_OrgStructure - Person / OrganizationalStructure
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_Person_OrgStructure", "Person", "OwnerOrgStructId", "OrganizationalStructure", "Id");

                // ProfileGroupAccessRequest
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_ProfileGroupAccessRequest_TempProfileGroup", "ProfileGroupAccessRequest", "TemporaryProfileId", "ProfileGroup", "Id");
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_ProfileGroupAccessRequest_PortalUserRequest", "ProfileGroupAccessRequest", "PortalUserRequestId", "PortalUser", "Id");
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_ProfileGroupAccessRequest_PortalUserReceived", "ProfileGroupAccessRequest", "PortalUserReceivedId", "PortalUser", "Id");
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_ProfileGroupAccessRequest_ProfileGroupRequest", "ProfileGroupAccessRequest", "ProfileGroupRequestId", "ProfileGroup", "Id");
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_ProfileGroupAccessRequest_ProfileGroupReceived", "ProfileGroupAccessRequest", "ProfileGroupReceivedId", "ProfileGroup", "Id");

                // PortalUserFingerprint - PortalUser
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_PortalUserFingerprint_PortalUser", "PortalUserFingerprint", "UserId", "PortalUser", "Id");

                // OrgStructConfigDefault - OrgStructConfigDefault
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_OrgStructConfig_OrgStructConfigDependent", "OrgStructConfigDefault", "OrgStructConfigDefaultDependencyId", "OrgStructConfigDefault", "Id");

                // OrganizationalStructureConfig - OrganizationalStructure / OrgStructConfigDefault
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_OrgStrucConfig_OrgStruc", "OrganizationalStructureConfig", "OrganizationalStructureId", "OrganizationalStructure", "Id");
                DataReferenceMapManager.CreateForeignKey(migration, schema, "FK_OrgStrucConfig_Config", "OrganizationalStructureConfig", "ConfigId", "OrgStructConfigDefault", "Id");
            }
        }
    }
}
