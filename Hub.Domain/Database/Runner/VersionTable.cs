using FluentMigrator.Runner.VersionTableInfo;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure;
using Hub.Shared.Interfaces.MultiTenant;

namespace Hub.Domain.Database.Runner
{
    [VersionTableMetaData]
    public class VersionTable : DefaultVersionTableMetaData
    {
        public override string SchemaName
        {
            get
            {
                var schema = Singleton<ISchemaNameProvider>.Instance.TenantName();

                if (schema != "system")
                {
                    schema = "sch" + Engine.Resolve<ITenantManager>().GetInfo().Id;
                }

                return schema;
            }
        }

        public override string TableName
        {
            get
            {
                return "MigrationInfo";
            }
        }
    }
}
