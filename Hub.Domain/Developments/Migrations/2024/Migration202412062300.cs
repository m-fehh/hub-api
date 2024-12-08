using FluentMigrator;
using Hub.Domain.Database.Runner;
using Hub.Infrastructure;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Extensions;
using Hub.Shared.Interfaces.MultiTenant;

namespace Hub.Domain.Developments.Migrations._2024
{
    [Migration(202412062300)]
    public class Migration202412062300 : Migration
    {
        public override void Down() { }

        public override void Up()
        {
            var schema = Singleton<ISchemaNameProvider>.Instance.TenantName();

            if (schema == "system") return;

            schema = "sch" + Engine.Resolve<ITenantManager>().GetInfo().Id;

            // OrganizationalStructure
            Create.Sequence("SQ_OrganizationalStructure").InSchema(schema);
            Create.Table("OrganizationalStructure").InSchema(schema)
                .WithIdColumn("PK_OrganizationalStructure")
                .WithColumn("Abbrev").AsString(20).NotNullable()
                .WithColumn("Description").AsString(150).NotNullable()
                .WithColumn("Inactive").AsBoolean().NotNullable()
                .WithColumn("IsRoot").AsBoolean().NotNullable()
                .WithColumn("IsDomain").AsBoolean().NotNullable()
                .WithColumn("IsLeaf").AsBoolean().NotNullable()
                .WithColumn("FatherId").AsInt64().Nullable()
                .WithColumn("Tree").AsString(600).Nullable()
                .WithColumn("ExternalCode").AsString(100).Nullable()
                .WithColumn("CreationUTC").AsDateTime().Nullable()
                .WithColumn("LastUpdateUTC").AsDateTime().Nullable();

            // PortalUser_OrgStructure
            Create.Table("PortalUser_OrgStructure").InSchema(schema)
                .WithColumn("UserId").AsInt64().NotNullable().PrimaryKey("PK_PortalUser_OrgStructure")
                .WithColumn("StructureId").AsInt64().NotNullable().PrimaryKey("PK_PortalUser_OrgStructure");

            // PortalUser
            Create.Sequence("SQ_PortalUser").InSchema(schema);
            Create.Table("PortalUser").InSchema(schema)
                .WithIdColumn("PK_PortalUser")
                .WithColumn("PersonId").AsInt64().NotNullable()
                .WithColumn("Name").AsString(150).NotNullable()
                .WithColumn("Email").AsString(150).NotNullable()
                .WithColumn("Login").AsString(50).NotNullable()
                .WithColumn("Password").AsString(50).NotNullable()
                .WithColumn("TempPassword").AsString(50).Nullable()
                .WithColumn("CpfCnpj").AsString(100).NotNullable()
                .WithColumn("Gender").AsInt16().NotNullable()
                .WithColumn("BirthDate").AsDateTime().NotNullable()
                .WithColumn("AreaCode").AsString(5).NotNullable()
                .WithColumn("PhoneNumber").AsString(10).NotNullable()
                .WithColumn("Keyword").AsAnsiString(100).Nullable()
                .WithColumn("Inactive").AsBoolean().NotNullable()
                .WithColumn("ProfileId").AsInt64().NotNullable()
                .WithColumn("OwnerOrgStructId").AsInt64().NotNullable()
                .WithColumn("DefaultOrgStructureId").AsInt64().NotNullable()
                .WithColumn("ExternalCode").AsString(100).Nullable()
                .WithColumn("CreationUTC").AsDateTime().Nullable()
                .WithColumn("LastUpdateUTC").AsDateTime().Nullable();

            // AccessRule
            Create.Sequence("SQ_AccessRule").InSchema(schema);
            Create.Table("AccessRule").InSchema(schema)
                .WithIdColumn("PK_AccessRule")
                .WithColumn("ParentId").AsInt64().Nullable()
                .WithColumn("Description").AsString(150).NotNullable()
                .WithColumn("KeyName").AsString(150).NotNullable()
                .WithColumn("ForAdministratorOnly").AsByte().NotNullable()
                .WithColumn("Tree").AsString(600).Nullable();

            // ProfileGroup_Rule
            Create.Table("ProfileGroup_Rule").InSchema(schema)
                .WithColumn("ProfileGroupId").AsInt64().NotNullable().PrimaryKey("PK_ProfileGroup_Rule")
                .WithColumn("AccessRuleId").AsInt64().NotNullable().PrimaryKey("PK_ProfileGroup_Rule");

            // ProfileGroup
            Create.Sequence("SQ_ProfileGroup").InSchema(schema);
            Create.Table("ProfileGroup").InSchema(schema)
                .WithIdColumn("PK_ProfileGroup")
                .WithColumn("Name").AsString(150).NotNullable()
                .WithColumn("OwnerOrgStructId").AsInt64().Nullable()
                .WithColumn("DaysToInactivate").AsInt16().Nullable()
                .WithColumn("PasswordExpirationDays").AsInt32().WithDefaultValue(90).NotNullable()
                .WithColumn("Administrator").AsByte().NotNullable();

            // Person
            Create.Sequence("SQ_Person").InSchema(schema);
            Create.Table("Person").InSchema(schema)
                .WithIdColumn("PK_Person")
                .WithColumn("Document").AsString(20).NotNullable()
                .WithColumn("Name").AsString(500).NotNullable()
                .WithColumn("ExternalCode").AsString(100).Nullable()
                .WithColumn("OwnerOrgStructId").AsInt64().NotNullable()
                .WithColumn("CreationUTC").AsDateTime().Nullable()
                .WithColumn("LastUpdateUTC").AsDateTime().Nullable();

            // Person_OrgStructure
            Create.Table("Person_OrgStructure").InSchema(schema)
                .WithColumn("PersonId").AsInt64().NotNullable().PrimaryKey("PK_Person_OrgStructure")
                .WithColumn("StructureId").AsInt64().NotNullable().PrimaryKey("PK_Person_OrgStructure");

            // PortalUserPassHistory
            Create.Sequence("SQ_PortalUserPassHistory").InSchema(schema);
            Create.Table("PortalUserPassHistory").InSchema(schema)
                .WithIdColumn("PK_PortalUserPassHistory")
                .WithColumn("UserId").AsInt64().Nullable()
                .WithColumn("Password").AsString(100).Nullable()
                .WithColumn("CreationUTC").AsDateTime().NotNullable()
                .WithColumn("ExpirationUTC").AsDateTime().Nullable();

            // PortalUserSetting
            Create.Sequence("SQ_PortalUserSetting");
            Create.Table("PortalUserSetting").InSchema(schema)
                .WithIdColumn("PK_PortalUserSetting")
                .WithColumn("PortalUserId").AsInt64().NotNullable()
                .WithColumn("Name").AsString(150).NotNullable()
                .WithColumn("Value").AsString(2000).NotNullable();

            #region Foreign

            ForeignKeyMap.Map(this);

            #endregion
        }
    }
}
