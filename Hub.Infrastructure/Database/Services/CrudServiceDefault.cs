using Hub.Infrastructure.Database.NhManagement;
using Hub.Shared.Interfaces;

namespace Hub.Infrastructure.Database.Services
{
    /// <summary>
    /// Implementação básica do serviço de CRUD. Essa classe pode ser usada para facilitar a implementação da interface <see cref="ICrudService"/>
    /// </summary>
    /// <typeparam name="T">Entidade cuidada pela crud dessa classe</typeparam>
    public class CrudServiceDefault<T> : CrudService<T>
        where T : class, IBaseEntity
    {
        protected event EventHandler<T> OnBeforeInsert;
        protected event EventHandler<T> OnBeforeUpdate;
        protected event EventHandler<long> OnBeforeDelete;

        public CrudServiceDefault(IRepository<T> repository) : base(repository)
        {
        }

        public override long Insert(T entity)
        {
            OnBeforeInsert?.Invoke(this, entity);

            using (var transaction = base._repository.BeginTransaction())
            {
                var ret = base._repository.Insert(entity);

                if (transaction != null) base._repository.Commit();

                return ret;
            }
        }

        public override void Update(T entity)
        {
            OnBeforeUpdate?.Invoke(this, entity);

            using (var transaction = base._repository.BeginTransaction())
            {
                base._repository.Update(entity);

                if (transaction != null) base._repository.Commit();
            }
        }

        public override void Delete(long id)
        {
            OnBeforeDelete?.Invoke(this, id);

            using (var transaction = base._repository.BeginTransaction())
            {
                base._repository.Delete(id);

                if (transaction != null) base._repository.Commit();
            }
        }
    }
}
