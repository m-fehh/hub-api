using NHibernate;

namespace Hub.Infrastructure.Database.NhManagement
{
    internal class NhTransaction : IDisposable
    {
        private ITransaction _transaction;

        public NhTransaction(ITransaction transaction)
        {
            _transaction = transaction;
        }

        public virtual void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            catch (Exception ex)
            {
                DataReferenceMapManager.FindFKException(ex);
            }
        }

        public virtual void Rollback()
        {
            _transaction.Rollback();
        }

        public bool IsActive
        {
            get { return _transaction == null ? false : _transaction.IsActive; }
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                if (_transaction.IsActive && !_transaction.WasCommitted && !_transaction.WasRolledBack)
                {
                    _transaction.Rollback();
                }

                _transaction.Dispose();

                _transaction = null;
            }
        }
    }
}
