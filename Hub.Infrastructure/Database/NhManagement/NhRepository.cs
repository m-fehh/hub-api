using Hub.Infrastructure.Logger.Interfaces;
using Hub.Shared.Enums.Infrastructure;
using Hub.Shared.Interfaces.Logger;
using Hub.Shared.Interfaces;
using NHibernate.Linq;
using NHibernate;
using Hub.Infrastructure.Autofac.Dependency;
using Hub.Shared.Log;


namespace Hub.Infrastructure.Database.NhManagement
{
    /// <summary>
    /// Classes que implementam essa interface intermedeiam a entidade na aplicação e o banco de dados.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade a ser controlada pela classe</typeparam>
    public interface IRepository<T> : IRepository
        where T : IBaseEntity
    {
        long Insert(T entity);
        long StatelessInsert(T entity);
        void Update(T entity);
        void StatelessUpdate(T entity);
        void Delete(long id);
        void Delete(T entity);
        T StatelessGetById(long id);
        T GetById(long id);
        T LoadById(long id);
        IQueryable<T> CacheableTable { get; }
        IQueryable<T> Table { get; }
        IQueryable<T> StatelessTable { get; }
        IQueryable<T> LifetimeScopeTable { get; }
        ICriteria CreateCriteria();
    }

    public interface IRepository : ISetType
    {
        IDisposable BeginTransaction();
        IDisposable BeginStatelessTransaction();
        IDisposable BeginTransaction(System.Data.IsolationLevel isolationLevel);
        void Commit();
        void CommitStateless();
        void RollBack();
        object Refresh(object entity);
        IQuery CreateQuery(string hql);
        IQuery CreateStatelessQuery(string hql);
        ISQLQuery CreateSQLQuery(string sql);
        ISQLQuery CreateStatelessSQLQuery(string sql);
        void Flush();
        void Clear();
        void Evict(object entity);
        bool IsInitialized(object o);
    }

    public class IgnoreModificationControl
    {
        public bool Ignore { get; set; }
    }

    internal class NhRepository<T> : NhRepository, IRepository<T>
               where T : class, IBaseEntity
    {
        public NhRepository() : base()
        {
        }

        public NhRepository(string tenantName) : base(tenantName)
        {
        }

        public T StatelessGetById(long id)
        {
            if (_resolvedType != null)
            {
                var methodInfo = typeof(ISession).GetMethod("Get", new[] { typeof(object) }).MakeGenericMethod(_resolvedType);

                return (T)methodInfo.Invoke(NhSessionProvider.StatelessSession.Value, new object[] { id });
            }
            else
            {
                return NhSessionProvider.StatelessSession.Value.Get<T>(id);
            }
        }

        public T GetById(long id)
        {
            if (_resolvedType != null)
            {
                var methodInfo = typeof(ISession).GetMethod("Get", new[] { typeof(object) }).MakeGenericMethod(_resolvedType);

                return (T)methodInfo.Invoke(NhSessionProvider.CurrentSession(tenantName), new object[] { id });
            }
            else
            {
                return NhSessionProvider.CurrentSession(tenantName).Get<T>(id);
            }
        }

        public T LoadById(long id)
        {
            if (_resolvedType != null)
            {
                var methodInfo = typeof(ISession).GetMethod("Load", new[] { typeof(object) }).MakeGenericMethod(_resolvedType);

                return (T)methodInfo.Invoke(NhSessionProvider.CurrentSession(tenantName), new object[] { id });
            }
            else
            {
                return NhSessionProvider.CurrentSession(tenantName).Load<T>(id);
            }
        }

        public long Insert(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (!NhSessionProvider.CurrentSession(tenantName).Transaction.IsActive)
                throw new Exception("Nenhuma transação ativa");

            object retObj = null;

            if (entity is IModificationControl)
            {
                if (!Engine.Resolve<IgnoreModificationControl>().Ignore)
                {
                    (entity as IModificationControl).CreationUTC = DateTime.UtcNow;
                    (entity as IModificationControl).LastUpdateUTC = DateTime.UtcNow;
                }
            }

            if (_resolvedType != null)
            {
                retObj = NhSessionProvider.CurrentSession(tenantName).Save(Convert.ChangeType(entity, _resolvedType));
            }
            else
            {
                retObj = NhSessionProvider.CurrentSession(tenantName).Save(entity);
            }

            Log(entity, ELogAction.Insertion);

            if (retObj is long)
            {
                return (long)retObj;
            }
            else
            {
                return 0;
            }
        }

        public long StatelessInsert(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (!NhSessionProvider.StatelessSession.Value.Transaction.IsActive)
                throw new Exception("Nenhuma transação ativa");

            object retObj = null;

            if (entity is IModificationControl)
            {
                if (!Engine.Resolve<IgnoreModificationControl>().Ignore)
                {
                    (entity as IModificationControl).CreationUTC = DateTime.UtcNow;
                    (entity as IModificationControl).LastUpdateUTC = DateTime.UtcNow;
                }
            }

            if (_resolvedType != null)
            {
                retObj = NhSessionProvider.StatelessSession.Value.Insert(Convert.ChangeType(entity, _resolvedType));
            }
            else
            {
                retObj = NhSessionProvider.StatelessSession.Value.Insert(entity);
            }

            if (retObj is long)
            {
                return (long)retObj;
            }
            else
            {
                return 0;
            }
        }

        public void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (!NhSessionProvider.CurrentSession(tenantName).Transaction.IsActive)
                throw new Exception("Nenhuma transação ativa");

            if (entity is IModificationControl)
            {
                if (!Engine.Resolve<IgnoreModificationControl>().Ignore)
                {
                    (entity as IModificationControl).LastUpdateUTC = DateTime.UtcNow;
                }
            }

            if (_resolvedType != null)
            {
                NhSessionProvider.CurrentSession(tenantName).Update(Convert.ChangeType(entity, _resolvedType));
            }
            else
            {
                NhSessionProvider.CurrentSession(tenantName).Update(entity);
            }

            Log(entity, ELogAction.Update);
        }

        public void StatelessUpdate(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (!NhSessionProvider.StatelessSession.Value.Transaction.IsActive)
                throw new Exception("Nenhuma transação ativa");

            if (entity is IModificationControl)
            {
                if (!Engine.Resolve<IgnoreModificationControl>().Ignore)
                {
                    (entity as IModificationControl).LastUpdateUTC = DateTime.UtcNow;
                }
            }

            if (_resolvedType != null)
            {
                NhSessionProvider.StatelessSession.Value.Update(Convert.ChangeType(entity, _resolvedType));
            }
            else
            {
                NhSessionProvider.StatelessSession.Value.Update(entity);
            }

            Log(entity, ELogAction.Update);
        }

        public void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (!NhSessionProvider.CurrentSession(tenantName).Transaction.IsActive)
                throw new Exception("Nenhuma transação ativa");

            if (_resolvedType != null)
            {
                NhSessionProvider.CurrentSession(tenantName).Delete(Convert.ChangeType(entity, _resolvedType));
            }
            else
            {
                NhSessionProvider.CurrentSession(tenantName).Delete(entity);
            }

            Log(entity, ELogAction.Deletion);
        }

        public void Delete(long id)
        {
            if (!NhSessionProvider.CurrentSession(tenantName).Transaction.IsActive)
                throw new Exception("Nenhuma transação ativa");

            if (_resolvedType != null)
            {
                var methodInfo = typeof(ISession).GetMethod("Load", new[] { typeof(object) }).MakeGenericMethod(_resolvedType);

                var entity = methodInfo.Invoke(NhSessionProvider.CurrentSession(tenantName), new object[] { id });

                NhSessionProvider.CurrentSession(tenantName).Delete(entity);

                Log((T)entity, ELogAction.Deletion);
            }
            else
            {
                T entity = NhSessionProvider.CurrentSession(tenantName).Load<T>(id);

                NhSessionProvider.CurrentSession(tenantName).Delete(entity);

                Log(entity, ELogAction.Deletion);
            }
        }

        public IQueryable<T> CacheableTable
        {
            get
            {
                return Table.WithOptions(a => a.SetCacheable(true));
            }
        }

        public IQueryable<T> Table
        {
            get
            {
                if (_resolvedType != null)
                {
                    var methods = from m in typeof(ISession).GetMethods()
                                  where m.Name == "Query"
                                     && m.IsGenericMethodDefinition
                                     && m.GetParameters().Count() == 0
                                  select m;

                    var methodInfo = methods.First().MakeGenericMethod(_resolvedType);

                    return (IQueryable<T>)methodInfo.Invoke(NhSessionProvider.CurrentSession(tenantName), null);
                }
                else
                {
                    return NhSessionProvider.CurrentSession(tenantName).Query<T>();
                }
            }
        }

        public IQueryable<T> StatelessTable
        {
            get
            {

                if (_resolvedType != null)
                {
                    var methods = from m in typeof(ISession).GetMethods()
                                  where m.Name == "Query"
                                     && m.IsGenericMethodDefinition
                                     && m.GetParameters().Count() == 0
                                  select m;

                    var methodInfo = methods.First().MakeGenericMethod(_resolvedType);

                    return (IQueryable<T>)methodInfo.Invoke(NhSessionProvider.StatelessSession.Value, null);
                }
                else
                {
                    return NhSessionProvider.StatelessSession.Value.Query<T>();
                }
            }
        }

        public IQueryable<T> LifetimeScopeTable
        {
            get
            {
                var session = NhSessionProvider.CurrentLifetimeScopeSession;

                if (_resolvedType != null)
                {
                    var methods = from m in typeof(ISession).GetMethods()
                                  where m.Name == "Query"
                                     && m.IsGenericMethodDefinition
                                     && m.GetParameters().Count() == 0
                                  select m;

                    var methodInfo = methods.First().MakeGenericMethod(_resolvedType);

                    return (IQueryable<T>)methodInfo.Invoke(session, null);
                }
                else
                {
                    return session.Query<T>();
                }
            }
        }

        public ICriteria CreateCriteria()
        {
            if (_resolvedType != null)
            {
                var methodInfo = typeof(ISession).GetMethod("CreateCriteria", new Type[] { }).MakeGenericMethod(_resolvedType);

                return (ICriteria)methodInfo.Invoke(NhSessionProvider.CurrentSession(tenantName), null);
            }
            else
            {
                return NhSessionProvider.CurrentSession(tenantName).CreateCriteria<T>();
            }
        }

        private void Log(T entity, ELogAction action)
        {
            if (Engine.Resolve<IgnoreLogScope>().Ignore) return;

            if (_logManager == null) Engine.TryResolve<ILogManager>(out _logManager);

            if (_logManager != null)
            {

                var log = _logManager.Audit(entity, action, true, action == ELogAction.Update);

                if (log != null) NhSessionProvider.CurrentSession(tenantName).Save(Convert.ChangeType(log, Engine.Resolve<ILog>().GetType()));

            }
        }
    }

    internal class NhRepository : IRepository
    {
        protected string tenantName;
        protected NhTransaction _transaction;
        protected ILogManager _logManager;
        protected Type _resolvedType;

        public NhRepository()
        {
        }

        public NhRepository(string tenantName)
        {
            this.tenantName = tenantName;
        }

        public object Refresh(object entity)
        {
            try
            {
                if (_resolvedType != null && IsInitialized(entity))
                {
                    NhSessionProvider.CurrentSession(tenantName).Refresh(Convert.ChangeType(entity, _resolvedType));
                }
                else
                {

                    if (entity is IBaseEntity)
                    {
                        var id = (entity as IBaseEntity).Id;

                        if (id != 0)
                        {
                            entity = NhSessionProvider.CurrentSession(tenantName).Get(entity.GetType(), id);
                        }
                    }
                    else
                    {
                        NhSessionProvider.CurrentSession(tenantName).Refresh(entity);
                    }
                }
            }
            catch (HibernateException)
            {
                //workarround para não causar exception se tentar dar um refresh de um objeto desatachado
                try
                {
                    NhSessionProvider.CurrentSession(tenantName).Refresh(entity);
                }
                catch (HibernateException)
                {

                }
            }

            return entity;
        }

        public IDisposable BeginTransaction()
        {
            if (NhSessionProvider.CurrentSession(tenantName).Transaction.IsActive) return null;

            _transaction = new NhTransaction(NhSessionProvider.CurrentSession(tenantName).BeginTransaction());

            return _transaction;
        }

        public IDisposable BeginStatelessTransaction()
        {
            if (NhSessionProvider.StatelessSession.Value.Transaction.IsActive) return null;

            _transaction = new NhTransaction(NhSessionProvider.StatelessSession.Value.BeginTransaction());

            return _transaction;
        }

        public IDisposable BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            if (NhSessionProvider.CurrentSession(tenantName).Transaction.IsActive) return null;

            _transaction = new NhTransaction(NhSessionProvider.CurrentSession(tenantName).BeginTransaction(isolationLevel));

            return _transaction;
        }

        public void Commit()
        {
            _transaction.Commit();

            _transaction.Dispose();

            _transaction = null;
        }

        public void CommitStateless()
        {
            if (NhSessionProvider.StatelessSession.Value.Transaction.IsActive)
            {
                NhSessionProvider.StatelessSession.Value.Transaction.Commit();
            }
        }

        public void RollBack()
        {
            _transaction.Dispose();

            _transaction = null;
        }

        public IQuery CreateQuery(string hql)
        {
            return NhSessionProvider.CurrentSession(tenantName).CreateQuery(hql);
        }

        public ISQLQuery CreateSQLQuery(string sql)
        {
            return NhSessionProvider.CurrentSession(tenantName).CreateSQLQuery(sql);
        }

        public IQuery CreateStatelessQuery(string hql)
        {
            return NhSessionProvider.StatelessSession.Value.CreateQuery(hql);
        }

        public ISQLQuery CreateStatelessSQLQuery(string sql)
        {
            return NhSessionProvider.StatelessSession.Value.CreateSQLQuery(sql);
        }

        public void Flush()
        {
            NhSessionProvider.CurrentSession(tenantName).Flush();
        }

        public void Clear()
        {
            NhSessionProvider.CurrentSession(tenantName).Clear();
        }

        public void Evict(object entity)
        {
            NhSessionProvider.CurrentSession(tenantName).Evict(entity);
        }

        public bool IsInitialized(object o)
        {
            if (o == null) return false;

            return NHibernateUtil.IsInitialized(o);
        }

        #region ISetType Members

        /// <summary>
        /// Ao utiliar o repositório com uma interface como parametro, o método SetType será invocado pelo Autofac para passar o tipo resolvido da interface, 
        /// e então todos os demais métodos do repositório deverão considerá-la para não passar uma interface para a sessão
        /// </summary>
        /// <param name="resolvedType"></param>
        public void SetType(Type resolvedType)
        {
            _resolvedType = resolvedType;
        }

        #endregion
    }
}
