﻿using FluentMigrator;
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
                .WithColumn("Inactive").AsBoolean().NotNullable()
                .WithColumn("ProfileId").AsInt64().NotNullable()
                .WithColumn("OwnerOrgStructId").AsInt64().Nullable()
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
                .WithColumn("Administrator").AsByte().NotNullable();

            // Person
            Create.Sequence("SQ_Person").InSchema(schema);
            Create.Table("Person").InSchema(schema)
                .WithIdColumn("PK_Person")
                .WithColumn("CpfCnpj").AsString(100).NotNullable()
                .WithColumn("Name").AsString(500).NotNullable()
                .WithColumn("Gender").AsInt16().NotNullable()
                .WithColumn("BirthDate").AsDate().NotNullable()
                .WithColumn("AreaCode").AsString(5).NotNullable()
                .WithColumn("Number").AsString(10).NotNullable()
                .WithColumn("CreationUTC").AsDateTime().Nullable()
                .WithColumn("LastUpdateUTC").AsDateTime().Nullable();


            #region Foreign

            ForeignKeyMap.Map(this);

            #endregion
        }
    }
}
