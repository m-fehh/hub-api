namespace Hub.Infrastructure.Database.NhManagement
{
    public interface INhStatelessSessionScope : IDisposable
    {
        bool IsOpenedSession();
    }

    public class NhStatelessSessionScope : INhStatelessSessionScope
    {
        [ThreadStatic]
        private static bool _created = false;

        public NhStatelessSessionScope()
        {
            if (NhSessionProvider.StatelessSession.Value == null)
            {
                NhSessionProvider.StatelessSession.Value = NhSessionProvider.NewStatelessSession();

                _created = true;
            }
        }

        public bool IsOpenedSession()
        {
            return NhSessionProvider.StatelessSession.Value != null;
        }

        public void Dispose()
        {
            if (_created)
            {
                NhSessionProvider.StatelessSession.Value.Dispose();

                NhSessionProvider.StatelessSession.Value = null;

                _created = false;
            }
        }
    }
}
