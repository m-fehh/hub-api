using FluentMigrator;
using Hub.Infrastructure;
using Hub.Shared.Interfaces.MultiTenant;

namespace Hub.Domain.Database.Runner
{
    public static class ForeignKeyMap
    {
        public static void Map(MigrationBase migration)
        {
            var schema = "sch" + Engine.Resolve<ITenantManager>().GetInfo().Id;
        }
    }
}
