using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Database.Services;
using Hub.Infrastructure.Extensions;
using Hub.Shared.Interfaces;
using Newtonsoft.Json;

namespace Hub.Infrastructure.Mapper
{
    public interface IModelEntityMapper<TEntity, TModel> where TEntity : IBaseEntity where TModel : ICrudModel
    {
        TModel BuildModel(TEntity entity);
        TEntity BuildEntity(TModel model, long? id = null);
    }

    public class ModelEntityMapper<TEntity, TModel> : IModelEntityMapper<TEntity, TModel>
        where TEntity : IBaseEntity
        where TModel : ICrudModel
    {
        private readonly IRepository<TEntity> repository;

        public ModelEntityMapper(IRepository<TEntity> repository)
        {
            this.repository = repository;
        }

        /// <summary>
        /// Constrói o modelo a partir de uma entidade (substitui o AutoMapper)
        /// Este mapeamento ignora as listas
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public TModel MapModel(TEntity entity, TModel model = default(TModel))
        {
            if (model == null)
            {
                model = Activator.CreateInstance<TModel>();
            }

            foreach (var mPi in typeof(TModel).GetProperties().Where(m =>
                m.SetMethod != null &&
                (m.PropertyType.IsGenericType == false || (m.PropertyType.IsGenericType &&
                 m.PropertyType.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(System.Collections.IList)) == false))))
            {
                var ePi = typeof(TEntity).GetProperty(mPi.Name);

                if (ePi != null)
                {
                    if (ePi.GetMethod != null && ePi.CustomAttributes.Any(c => c.AttributeType == typeof(NHibernate.Mapping.Attributes.OneToManyAttribute)) == false)
                    {
                        mPi.SetValue(model, ePi.GetValue(entity));
                    }
                }
                else if (mPi.Name.EndsWith("_Id"))
                {
                    ePi = typeof(TEntity).GetProperty(mPi.Name.Replace("_Id", ""));

                    if (ePi != null && ePi.GetMethod != null && typeof(IBaseEntity).IsAssignableFrom(ePi.PropertyType))
                    {
                        var reference = (IBaseEntity)ePi.GetValue(entity);

                        if (reference != null)
                        {
                            mPi.SetValue(model, reference.Id);
                        }
                        else
                        {
                            mPi.SetValue(model, null);
                        }
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// Constrói a entidade a partir de um modelo (substitui o AutoMapper)
        /// Este mapeamento ignora as listas
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public TEntity MapEntity(TModel model, TEntity entity = default(TEntity))
        {
            if (entity == null)
            {
                entity = Activator.CreateInstance<TEntity>();
            }

            foreach (var mPi in typeof(TModel).GetProperties().Where(m =>
                m.GetMethod != null &&
                (m.PropertyType.IsGenericType == false || (m.PropertyType.IsGenericType &&
                 m.PropertyType.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(System.Collections.IList)) == false))))
            {
                var ePi = typeof(TEntity).GetProperty(mPi.Name);

                if (ePi != null)
                {
                    if (ePi.SetMethod != null && ePi.CustomAttributes.Any(c => c.AttributeType == typeof(NHibernate.Mapping.Attributes.OneToManyAttribute)) == false)
                    {
                        ePi.SetValue(entity, mPi.GetValue(model));
                    }
                }
                else if (mPi.Name.EndsWith("_Id"))
                {
                    ePi = typeof(TEntity).GetProperty(mPi.Name.Replace("_Id", ""));

                    if (ePi != null && ePi.SetMethod != null && typeof(IBaseEntity).IsAssignableFrom(ePi.PropertyType))
                    {
                        var referenceId = (long?)mPi.GetValue(model);

                        if (referenceId != null)
                        {
                            var reference = (IBaseEntity)Activator.CreateInstance(ePi.PropertyType);

                            reference.Id = referenceId.Value;

                            ePi.SetValue(entity, reference);
                        }
                        else
                        {
                            ePi.SetValue(entity, null);
                        }
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Gera o modelo a partir da entidade
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public TModel BuildModel(TEntity entity)
        {
            TModel model = Activator.CreateInstance<TModel>();

            model = MapModel(entity, model);

            if (entity != null && entity.Id != 0)
            {
                model.SerializedOldValue = model.SerializeToJSON();
            }

            return model;
        }


        /// <summary>
        /// Gera a entidade a partir do modelo (
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public TEntity BuildEntity(TModel model, long? id = null)
        {
            if (model == null) return default(TEntity);

            TEntity entity = default(TEntity);

            TModel modelFromEntity = Activator.CreateInstance<TModel>();

            if (model.Id != null || id != null)
            {
                if (id == null) id = model.Id.Value;

                entity = repository.GetById(id.Value);

                modelFromEntity = MapModel(entity, modelFromEntity);
            }

            TModel oldModel = default(TModel);

            //model antes de ser modificado, serve de referencia para alterar apenas as propriedades alteradas
            if (model.SerializedOldValue != null)
            {
                oldModel = JsonConvert.DeserializeObject<TModel>(model.SerializedOldValue);
            }

            if (oldModel != null)
            {
                foreach (var pi in model.GetType().GetProperties().Where(m => m.SetMethod != null && !typeof(ICollection<>).IsAssignableFrom(m.PropertyType)))
                {
                    //neste ponto é feita uma procurar por todas as propriedades não alteradas do modelo
                    if ((pi.GetValue(model) == pi.GetValue(oldModel)) || (pi.GetValue(model) != null && pi.GetValue(model).Equals(pi.GetValue(oldModel))))
                    {
                        //para cada propriedade, copia o valor que veio do banco, assim não haverá update
                        pi.SetValue(model, pi.GetValue(modelFromEntity));
                    }
                }
            }

            //setar a entidade com os dados da model
            entity = MapEntity(model, entity);

            return entity;
        }
    }
}
