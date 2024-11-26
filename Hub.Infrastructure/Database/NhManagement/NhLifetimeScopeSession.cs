using NHibernate;

namespace Hub.Infrastructure.Database.NhManagement
{
    public interface ILifetimeScopeSession : IDisposable { }

    public class NhLifetimeScopeSession : ILifetimeScopeSession
    {
        private ISession session;

        public NhLifetimeScopeSession(IDatabaseInformation databaseInformation)
        {
            session = NhSessionProvider.NewSession();

            if (databaseInformation.DatabaseSupplier() == "sqlserver")
            {
                session.CreateSQLQuery("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED").ExecuteUpdate();
            }
        }

        public ISession GetSession()
        {
            return session;
        }

        public void Dispose()
        {
            session.Close();
        }
    }
}
