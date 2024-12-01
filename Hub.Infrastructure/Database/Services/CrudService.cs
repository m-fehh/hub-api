using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Mapper;
using Hub.Shared.Interfaces;
using System.Linq.Expressions;

namespace Hub.Infrastructure.Database.Services
{
    /// <summary>
    /// Interface que usada para implementar o serviço de CRUD de uma entidade.
    /// </summary>
    /// <typeparam name="T">Entidade cuidada pela crud</typeparam>
    public interface ICrudService<T> : ICrudService
        where T : IBaseEntity
    {
        T GetById(long id);
        long Insert(T entity);
        void Update(T entity);
        IQueryable<T> Table { get; }
        IQueryable<T> StatelessTable { get; }
        ISet<TItem> ChargeList<TFather, TItem>(TFather father, ISet<TItem> originalItens, IEnumerable<TItem> copyItens)
            where TItem : IBaseEntity, IListItemEntity
            where TFather : IBaseEntity;

        IQueryable<TResult> Get<TResult>(Expression<Func<T, bool>> wherePredicate,
                                        Expression<Func<T, TResult>> selectPredicate,
                                        int quantity = 0,
                                        bool useCacheableTable = false);
    }

    public interface ICrudService
    {
        void Delete(long id);
    }

    public interface ICrudService<T, M> : ICrudService<T>
    where T : IBaseEntity
    where M : ICrudModel
    {
        T Insert(M model);
        T Update(M model);
    }

    /// <summary>
    /// Interface que deve ser implementada por todos os models de telas CRUD
    /// </summary>
    public interface ICrudModel
    {
        long? Id { get; set; }

        string SerializedOldValue { get; set; }
    }


    /// <summary>
    /// Implementação básica do serviço de CRUD. Essa classe pode ser usada para facilitar a implementação da interface <see cref="ICrudService"/>
    /// </summary>
    /// <typeparam name="T">Entidade cuidada pela crud dessa classe</typeparam>
    public class CrudService<T> : ICrudService<T>
        where T : class, IBaseEntity
    {
        protected readonly IRepository<T> _repository;

        public CrudService(IRepository<T> repository)
        {
            this._repository = repository;
        }

        public virtual T GetById(long id)
        {
            return _repository.GetById(id);
        }

        public virtual long Insert(T entity)
        {
            throw new NotImplementedException();
        }

        public virtual void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Table
        {
            get { return _repository.Table; }
        }

        public object Refresh(object entity)
        {
            return _repository.Refresh(entity);
        }

        public IQueryable<T> StatelessTable
        {
            get { return _repository.StatelessTable; }
        }

        public void SaveList<TFather, TItem>(TFather father, IEnumerable<TItem> itens) where TItem : IBaseEntity, IListItemEntity where TFather : IBaseEntity
        {
            if (itens == null) return;

            foreach (var item in itens)
            {
                if (item.DeleteFromList)
                {
                    if (item.Id != 0)
                    {
                        Engine.Resolve<ICrudService<TItem>>().Delete(item.Id);
                    }
                }
                else
                {
                    if (item.Id == 0)
                    {
                        var propertyFather = item.GetType().GetProperties()
                            .First(pi => typeof(TFather) == pi.PropertyType || typeof(TFather).GetInterfaces().Contains(pi.PropertyType));

                        propertyFather.SetValue(item, father, null);

                        Engine.Resolve<ICrudService<TItem>>().Insert(item);
                    }
                    else
                    {
                        Engine.Resolve<ICrudService<TItem>>().Update(item);
                    }
                }
            }
        }

        public void DeleteList<TItem>(IEnumerable<TItem> itens) where TItem : IBaseEntity, IListItemEntity
        {
            if (itens == null) return;

            foreach (var item in itens)
            {
                if (item.Id != 0)
                {
                    Engine.Resolve<ICrudService<TItem>>().Delete(item.Id);
                }
            }
        }

        public ISet<TItem> ChargeList<TFather, TItem>(TFather father, ISet<TItem> originalItens, IEnumerable<TItem> copyItens)
            where TItem : IBaseEntity, IListItemEntity
            where TFather : IBaseEntity
        {
            //cria instâncias vazias caso as listas estiverem nulas
            if (originalItens == null) originalItens = new HashSet<TItem>();
            if (copyItens == null) copyItens = new HashSet<TItem>();

            //seleciona todos os itens da lista original que não estão contidos na lista a copiar
            foreach (TItem item in originalItens.Where(t => !copyItens.Select(c => c.Id).Contains(t.Id) && t.Id != 0))
            {
                //define que o item deverá ser excluído
                item.DeleteFromList = true;
            }

            //percorre a lista a ser copiada
            foreach (TItem oCopiar in copyItens)
            {
                //procura pelo objeto na lista original
                TItem oOriginal = originalItens.FirstOrDefault(t => t.Id == oCopiar.Id && oCopiar.Id != 0);

                if (oOriginal != null)
                {
                    //copia propriedade por propriedade para o objeto original
                    foreach (var pi in typeof(TItem).GetProperties())
                    {
                        //verifica apenas as propriedades que tenham o método set definido
                        if (pi.GetSetMethod() != null)
                        {
                            //para cada propriedade, copia o valor que veio do banco, assim não haverá update
                            pi.SetValue(oOriginal, pi.GetValue(oCopiar, null), null);
                        }
                    }
                }
                else
                {
                    //insere o novo item na lista
                    originalItens.Add(oCopiar);
                }
            }

            var propertyFather = typeof(TItem).GetProperties().First(pi => typeof(TFather) == pi.PropertyType || typeof(TFather).GetInterfaces().Contains(pi.PropertyType));

            foreach (TItem item in originalItens.Where(i => !i.DeleteFromList))
            {
                propertyFather.SetValue(item, father, null);
            }

            return originalItens;
        }

        public IQueryable<TResult> Get<TResult>(Expression<Func<T, bool>> wherePredicate, Expression<Func<T, TResult>> selectPredicate, int quantity = 0, bool useCacheableTable = false)
        {
            var query = (useCacheableTable ? _repository.CacheableTable : _repository.Table)
                                .Where(wherePredicate)
                                .Select(selectPredicate);

            if (quantity > 0)
                query = query.Take(quantity);

            return query;
        }
    }

    public class CrudService<T, M> : CrudService<T>, ICrudService<T, M>
        where T : class, IBaseEntity
        where M : ICrudModel
    {
        protected IModelEntityMapper<T, M> modelMapper;

        public CrudService(IRepository<T> repository)
            : base(repository)
        {
            modelMapper = Engine.Resolve<IModelEntityMapper<T, M>>();
        }

        public virtual T Insert(M model)
        {
            throw new NotImplementedException();
        }

        public virtual T Update(M model)
        {
            throw new NotImplementedException();
        }
    }
}
