namespace Hub.Infrastructure.Database.NhManagement.Migrations
{
    public interface IMigrationRunner
    {
        void MigrateToLatest();
    }
}
