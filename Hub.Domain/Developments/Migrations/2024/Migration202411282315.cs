using FluentMigrator;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure;
using Hub.Shared.Interfaces.MultiTenant;
using Hub.Infrastructure.Extensions;

namespace Hub.Domain.Developments.Migrations._2024
{
    [Migration(202411282315)]
    public class Migration202411282315 : Migration
    {
        public override void Down() { }

        public override void Up() 
        {
            var schema = Singleton<ISchemaNameProvider>.Instance.TenantName();

            if (schema == "system") return;

            schema = "sch" + Engine.Resolve<ITenantManager>().GetInfo().Id;

            if (!Schema.Schema(schema).Exists())
            {
                Create.Schema(schema);
            }

            // Execução das migrations
            Create.Sequence("SQ_StartUpDataExecuted").InSchema(schema);
            Create.Table("StartUpDataExecuted").InSchema(schema)
                .WithColumn("Id").AsInt64().PrimaryKey().Identity().WithColumnDescription("Identificador único da execução de dados de inicialização")
                .WithColumn("Name").AsString(100).NotNullable().WithColumnDescription("Nome da execução de dados de inicialização")
                .WithColumn("CreateDate").AsDate().NotNullable().WithColumnDescription("Data de criação da execução");

            // Tabelas de logs 
            Create.Sequence("SQ_Log").InSchema(schema);
            Create.Table("Log").InSchema(schema)
                .WithIdColumn("PK_Log")
                .WithColumn("CreateDate").AsDateTime().NotNullable()
                .WithColumn("CreateUserId").AsInt64().Nullable()
                .WithColumn("ObjectName").AsString(200).NotNullable()
                .WithColumn("ObjectId").AsInt64().NotNullable()
                .WithColumn("Action").AsString(20).NotNullable()
                .WithColumn("LogType").AsString(20).NotNullable()
                .WithColumn("Message").AsString(Int32.MaxValue).NotNullable()
                .WithColumn("OwnerOrgStructId").AsInt64().Nullable()
                .WithColumn("FatherId").AsInt64().Nullable()
                .WithColumn("IpAddress").AsAnsiString(50).Nullable();
        }
    }
}
