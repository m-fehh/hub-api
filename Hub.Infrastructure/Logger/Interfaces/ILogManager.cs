using Microsoft.AspNetCore.Http;
using NHibernate;
using NHibernate.Mapping.Attributes;
using NHibernate.Proxy;
using System.Collections;
using System.Configuration;
using System.Net;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Hub.Shared.Interfaces;
using Hub.Shared.Enums.Infrastructure;
using Hub.Shared.Interfaces.Logger;
using Hub.Infrastructure.Security;
using Hub.Infrastructure.Nominator;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Shared.Log;

namespace Hub.Infrastructure.Logger.Interfaces
{
    public interface ILogManager
    {
        ILog Audit(IBaseEntity obj, ELogAction action, bool verifyLogableEntity = true, bool deeper = true);
        void Error(Exception ex);
    }

    public class LogManager : ILogManager
    {
        protected virtual ILog InterceptLog(ILog log)
        {
            return log;
        }

        public ILog Audit(IBaseEntity obj, ELogAction action, bool verifyLogableEntity = true, bool deeper = true)
        {
            bool logsActived = true;

            HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            if (bool.TryParse(ConfigurationManager.AppSettings["LogsActived"], out logsActived))
            {
                if (!logsActived) return null;
            }

            if (obj == null) return null;

            if (verifyLogableEntity)
            {
                if (!typeof(ILogableEntity).IsAssignableFrom(obj.GetType())) return null;
            }

            //cria um cíclo de vida do autofac para gerenciamento da stateless session
            using (Engine.BeginLifetimeScope())
            {
                ILog log;

                if (Engine.TryResolve<ILog>(out log))
                {
                    log.Action = action;
                    log.CreateDate = DateTime.Now;
                    log.LogType = ELogType.Audit;

                    log.IpAddress = GetIp();

                    if (deeper)
                    {
                        log.Fields = GetFieldList(obj, log);

                        if (log.Fields.Count == 0) return null;
                    }

                    log.ObjectId = obj.Id;

                    if (typeof(ILogableEntityCustomName).IsAssignableFrom(obj.GetType()))
                    {
                        if (!string.IsNullOrEmpty((obj as ILogableEntityCustomName).CustomLogName))
                        {
                            log.ObjectName = (obj as ILogableEntityCustomName).CustomLogName;
                        }
                        else
                        {
                            log.ObjectName = Engine.Get(obj.GetType().Name.Replace("Proxy", ""));
                        }
                    }
                    else
                    {
                        log.ObjectName = Engine.Get(obj.GetType().Name.Replace("Proxy", ""));
                    }

                    log.CreateUser = Engine.Resolve<ISecurityProvider>().GetCurrent();
                    log.Message = Engine.Resolve<INominatorManager>().GetName(obj);

                    //tenta dar um refreh caso a mensagem esteja em branco, isso pode ocorrer quando há a tentativa de gravar um log de um objeto que tenha somente o id carregado
                    if (string.IsNullOrEmpty(log.Message))
                    {
                        using (Engine.BeginStatelessSessionScope())
                        {

                            var repository = Engine.Resolve(typeof(IRepository<>), obj.GetType());

                            var loadMethod = repository.GetType().GetMethod("StatelessGetById", new Type[] { typeof(long) });

                            var loadedObj = loadMethod.Invoke(repository, new object[] { obj.Id });

                            log.Message = Engine.Resolve<INominatorManager>().GetName(loadedObj);
                        }
                    }

                    log = InterceptLog(log);

                    return log;
                }
            }

            return null;
        }
        public string GetIp()
        {
            HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            string ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (ip == null || ip == "::1")
            {
                ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault().ToString();
            }
            return ip;
        }

        public void Error(Exception ex)
        {
            bool logsActived = true;

            if (bool.TryParse(ConfigurationManager.AppSettings["LogsActived"], out logsActived))
            {
                if (!logsActived) return;
            }

            ILog log;

            if (Engine.TryResolve<ILog>(out log))
            {
                var repo = Engine.Resolve<IRepository<ILog>>();

                log.Action = ELogAction.Insertion;
                log.CreateDate = DateTime.Now;
                log.CreateUser = Engine.Resolve<ISecurityProvider>().GetCurrent();
                log.LogType = ELogType.Error;
                log.ObjectId = ex.HResult;
                log.ObjectName = ex.Message;
                log.Message = ex.StackTrace;

                log = InterceptLog(log);

                using (var transaction = repo.BeginTransaction())
                {
                    repo.Insert(log);

                    if (transaction != null) repo.Commit();
                }
            }
        }

        private ISet<ILogField> GetFieldList<T>(T obj, ILog logFather) where T : IBaseEntity
        {
            var repository = Engine.Resolve(typeof(IRepository<>), (obj is INHibernateProxy ? NHibernateUtil.GetClass(obj) : obj.GetType()));

            var lifetimeScopeTable = (IQueryable)repository.GetType().GetProperty("LifetimeScopeTable").GetValue(repository);

            var isInitializedMethod = repository.GetType().GetMethod("IsInitialized", new Type[] { typeof(object) });

            var refreshMethod = repository.GetType().GetMethod("Refresh", new Type[] { typeof(object) });

            var nominator = Engine.Resolve<INominatorManager>();

            T oldObj = default(T);

            if (logFather.Action == ELogAction.Update)
            {
                oldObj = (T)((IEnumerable<object>)lifetimeScopeTable.Where("It.Id == @0", new object[] { obj.Id })).FirstOrDefault();
            }

            var ret = new HashSet<ILogField>();

            List<PropertyInfo> propertyList = (obj is INHibernateProxy ? NHibernateUtil.GetClass(obj) : obj.GetType()).GetProperties().Where(p =>
                  p.Name != "Id" &&
                  //A propriedade deve conter atributos do tipo BaseAttribute (ou seja, atributos do NHibernate que definem uma propriedade)
                  p.GetCustomAttributes(true).Where(a => a is BaseAttribute).Any() &&
                  //A propriedade não deve estar marcada com o atributo IgnoreLog
                  !p.GetCustomAttributes(true).Where(a => a is IgnoreLog).Any()).ToList();

            foreach (PropertyInfo prop in propertyList)
            {
                var newValue = prop.GetValue(obj);

                //não faz comparação de propriedades não inicializadas para não acionar o lazy load
                //por lógica, se não está inicializada é por que não foi manipulada e não precisa gravar log
                if (PrimitiveTypes.Test(prop.PropertyType) || (bool)isInitializedMethod.Invoke(repository, new object[] { newValue }))
                {
                    string newComparator = null, oldComparator = null;

                    bool isCollection = prop.PropertyType.GetInterfaces().Any(c => c.Name.StartsWith("ICollection") || (c.Name.StartsWith("IEnumerable") && c.FullName.Contains("Core.Entity")));

                    if (!isCollection)
                    {
                        if (typeof(IBaseEntity).IsAssignableFrom(prop.PropertyType))
                        {
                            newComparator = (prop.GetValue(obj) as IBaseEntity).Id.ToString();

                            if (oldObj != null)
                            {
                                var oldPropValue = prop.GetValue(oldObj);

                                if (oldPropValue != null)
                                {
                                    oldComparator = oldPropValue.GetType().GetProperty("Id").GetValue(oldPropValue).ToString();
                                }
                            }
                        }
                        else
                        {
                            var propValue = prop.GetValue(obj);
                            if (propValue != null)
                            {
                                if (prop.PropertyType == typeof(Decimal) || prop.PropertyType == typeof(Decimal?))
                                    newComparator = (propValue as Decimal?).Value.ToString("0.000000");
                                else
                                    newComparator = propValue.ToString();
                            }

                            if (oldObj != null)
                            {
                                var oldPropValue = prop.GetValue(oldObj);
                                if (oldPropValue != null)
                                {
                                    if (prop.PropertyType == typeof(Decimal) || prop.PropertyType == typeof(Decimal?))
                                        oldComparator = (oldPropValue as Decimal?).Value.ToString("0.000000");
                                    else
                                        oldComparator = oldPropValue.ToString();
                                }
                            }
                        }
                    }

                    //o log será gravado caso exista alguma modificação de valores ou se a ação for de inserção
                    //tambem grava um registro no caso das listas para indicar a quantidade de itens existentes
                    if (isCollection ||
                        newComparator != oldComparator ||
                        logFather.Action == ELogAction.Insertion)
                    {

                        if (!isCollection)
                        {
                            if (oldObj != null)
                            {
                                var oldPropValue = prop.GetValue(oldObj);

                                if (oldPropValue != null && !PrimitiveTypes.Test(prop.PropertyType) && !(bool)isInitializedMethod.Invoke(repository, new object[] { oldPropValue }))
                                {
                                    oldPropValue = refreshMethod.Invoke(repository, new object[] { oldPropValue });
                                }
                            }
                            var newPropValue = prop.GetValue(obj);

                            if (!PrimitiveTypes.Test(prop.PropertyType))
                            {
                                newPropValue = refreshMethod.Invoke(repository, new object[] { newPropValue });
                            }
                        }

                        var generateField = false;

                        //converte os valores a serem comparados para string
                        string newValueStr = nominator.GetPropertyDescritor(prop, obj);
                        string oldValueStr = nominator.GetPropertyDescritor(prop, oldObj, false);

                        var field = Engine.Resolve<ILogField>();

                        field.OldValue = oldValueStr;
                        field.NewValue = newValueStr;
                        field.PropertyName = Engine.Get(prop.Name);
                        field.Log = logFather;
                        field.Childs = new HashSet<ILog>();

                        //para listas, o sistema fará uma gravação de um ILog para cada objeto, indicando se o mesmo foi inserido, alterado ou excluído. 
                        //Por recursividade a gravação de cada log poderá gerar uma nova lista de ILogFields indicando as alterações dos registros.
                        if (isCollection)
                        {
                            if (prop.GetCustomAttributes(true).Any(a => a is DeeperLog))
                            {
                                IEnumerable<IBaseEntity> oldList;

                                if (oldObj != null)
                                {
                                    var oldValue = oldObj != null ? prop.GetValue(oldObj) : new HashSet<IBaseEntity>();

                                    oldList = (oldValue as IEnumerable).Cast<IBaseEntity>();
                                }
                                else
                                {
                                    oldList = new HashSet<IBaseEntity>();
                                }

                                var newList = (newValue as IEnumerable).Cast<IBaseEntity>();

                                if (prop.GetCustomAttributes(true).Where(a => a is ManyToManyAttribute).Any())
                                {
                                    foreach (IBaseEntity item in newList.Except(oldList))
                                    {
                                        ILog log = Audit(item, ELogAction.Insertion, false, false);

                                        if (log != null) field.Childs.Add(log);
                                    }

                                    foreach (IBaseEntity item in oldList.Except(newList))
                                    {
                                        ILog log = Audit(item, ELogAction.Deletion, false, false);

                                        if (log != null) field.Childs.Add(log);
                                    }

                                    if (field.Childs.Count > 0) generateField = true;
                                }
                                else
                                {
                                    foreach (IBaseEntity item in newList)
                                    {
                                        if (!(item is IListItemEntity) || (!(item as IListItemEntity).DeleteFromList))
                                        {
                                            ILog log = Audit(item, oldList.Contains(item) ? ELogAction.Update : ELogAction.Insertion, false, true);

                                            if (log != null) field.Childs.Add(log);
                                        }
                                    }
                                    foreach (IBaseEntity item in oldList.Except(newList.Where(o =>
                                    {
                                        if (o is IListItemEntity)
                                        {
                                            return !(o as IListItemEntity).DeleteFromList;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    })))
                                    {
                                        if (field.Childs == null) field.Childs = new HashSet<ILog>();

                                        var log = Audit(item, ELogAction.Deletion, false, false);

                                        field.Childs.Add(log);
                                    }

                                    if (field.Childs.Count > 0) generateField = true;
                                }
                            }
                        }
                        else if (typeof(IBaseEntity).IsAssignableFrom(prop.PropertyType))
                        {
                            if (prop.GetCustomAttributes(true).Any(a => a is DeeperLog))
                            {
                                if (field.Childs == null) field.Childs = new HashSet<ILog>();

                                var log = Audit(prop.GetValue(obj) as IBaseEntity, ELogAction.Update, false, true);

                                field.Childs.Add(log);
                            }

                            generateField = true;
                        }
                        else
                        {
                            generateField = true;
                        }

                        if (generateField)
                        {
                            ret.Add(field);
                        }
                    }
                }
            }

            return ret;
        }
    }
}
